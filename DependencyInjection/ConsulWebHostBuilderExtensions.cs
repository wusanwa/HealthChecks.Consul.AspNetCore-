using Consul;
using HealthChecks.Consul.AspNetCore;
using HealthChecks.Consul.AspNetCore.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using Winton.Extensions.Configuration.Consul;

namespace Microsoft.AspNetCore.Hosting
{
    public static class ConsulWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseConsulConfiguration(this IWebHostBuilder host, CancellationToken cancellationToken = default)
        {
            host.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var _fileName = "appsettings.Consul.json";
                var file = config.GetFileProvider().GetFileInfo(_fileName);
                if (file.Exists)
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile(_fileName);
            })
             .ConfigureAppConfiguration((hostingContext, config) =>
             {
                 var configuration = config.GetConsulConfiguration();
                 var isDev = hostingContext.HostingEnvironment.IsDevelopment();

                 var commonKey = $"{configuration.KVPrefix}/common";
                 var keyTmp = $"{configuration.KVPrefix}/{hostingContext.HostingEnvironment.ApplicationName}{configuration.KVProfileSeparator}";
                 using (var consulClient = configuration.MakeConsulClient())
                 {

                     var appKey = $"{ keyTmp }prod/{configuration.KVDataKey}";
                     var devKey = $"{ keyTmp }dev/{configuration.KVDataKey}";

                     AddConsulConfiguration(commonKey);

                     //SeekDefaultConsulConfiguration("appsettings.json", appKey);
                     //SeekDefaultConsulConfiguration("appsettings.Development.json", devKey);
                     AddConsulConfiguration(appKey);

                     if (isDev) AddConsulConfiguration(devKey);

                     void AddConsulConfiguration(string _appKey)
                     {
                         var _result = consulClient.KV.Get(_appKey).GetAwaiter().GetResult();

                         if (_result.Response != null)
                             config.AddConsul(_appKey, cancellationToken, opt =>
                             {
                                 opt.ConsulConfigurationOptions = consul =>
                                 {
                                     consul.Address = configuration.MakeConsulUri();
                                 };
                             });
                     }

                     //void SeekDefaultConsulConfiguration(string _fileName, string _appKey)
                     //{
                     //    if (configuration.KVUploadDefault)
                     //    {
                     //        var file = config.GetFileProvider().GetFileInfo(_fileName);
                     //        if (file.Exists)
                     //        {
                     //            var _result = consulClient.KV.Get(_appKey).GetAwaiter().GetResult();
                     //            if (_result.Response == null)
                     //                consulClient.KV.Put(new KVPair(_appKey)
                     //                {
                     //                    Value = StreamToBytes(file.CreateReadStream())
                     //                }).Wait();
                     //        }
                     //    }

                     //}
                 }
             });

            return host;
        }

        //private static byte[] StreamToBytes(Stream stream)
        //{
        //    byte[] bytes = new byte[stream.Length];
        //    stream.Read(bytes, 0, bytes.Length);
        //    // 设置当前流的位置为流的开始
        //    stream.Seek(0, SeekOrigin.Begin);
        //    return bytes;
        //}
    }
}
