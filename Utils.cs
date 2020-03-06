using Consul;
using HealthChecks.Consul.AspNetCore.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HealthChecks.Consul.AspNetCore
{
    public static class Utils
    {
        public static ConsulClient MakeConsulClient(this ConsulConfiguration configuration)
            => new ConsulClient(x => x.Address = configuration.MakeConsulUri());

        public static Uri MakeConsulUri(this ConsulConfiguration configuration)
        {
            var consulIpAdress = configuration.Host.GetIpAdresss() ?? throw new InvalidOperationException("无效的Consul服务器地址");
            return new Uri($"{configuration.Scheme}://{consulIpAdress}:{configuration.Port}");
        }
        internal static string GetIpAdresss(this string hostName)
        {
            var ips = Dns.GetHostAddresses(hostName);

            return ips.FirstOrDefault()?.ToString() ?? null;
        }
        internal static async Task<string> GetIpAdresssAsync(this string hostName)
        {
            var ips = await Dns.GetHostAddressesAsync(hostName);
            return ips.FirstOrDefault()?.MapToIPv4()?.ToString() ?? null;
        }
        internal static async Task<string> GetLinkHostLocalIpAdresssAsync(this string hostName)
        {
            try
            {
                var ips = await Dns.GetHostEntryAsync(hostName);
                return ips.AddressList.First().MapToIPv4().ToString() ;
            }
            catch
            {
                throw new InvalidProgramException($"Consul配置错误：无法访问目标地址{hostName}");
            }
        }
        internal static async Task<string> GetLocalIpAdresssAsync()
        {
            return await Dns.GetHostName().GetIpAdresssAsync() ?? "127.0.0.1";
        }
        internal static ConsulConfiguration GetConsulConfiguration(this IConfiguration configuration)
        {
            return configuration.GetSection("Consul").Get<ConsulConfiguration>();
        }
        internal static int GetlocalPort(this IConfiguration configuration)
        {
            return int.Parse(Regex.Match(configuration.GetSection("urls").Value, @"(http|https)://(.*)\:(\d+)").Result("$3"));
        }
        internal static ConsulConfiguration GetConsulConfiguration(this IConfigurationBuilder builder)
        {
            return builder.Build().GetSection("Consul").Get<ConsulConfiguration>();
        }
        internal static string ToMd5String(this string text)
        {

            var md5 = new MD5CryptoServiceProvider();

            var bytes = md5.ComputeHash(Encoding.Default.GetBytes(text));

            StringBuilder stringBuilder = new StringBuilder();

            foreach (var b in bytes)
                stringBuilder.Append(b.ToString("x2"));

            return stringBuilder.ToString();
        }

    }

}
