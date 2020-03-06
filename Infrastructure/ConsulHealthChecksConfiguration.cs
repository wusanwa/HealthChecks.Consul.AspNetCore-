using Consul;
using HealthChecks.Consul.AspNetCore.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace HealthChecks.Consul.AspNetCore.Infrastructure
{
    internal class ConsulHealthChecksConfiguration : IConsulHealthChecksConfiguration
    {
        readonly IApplicationLifetime _lifetime;
        readonly IHostingEnvironment _environment;
        readonly ConsulConfiguration _configuration;
        readonly int _localPort;
        public ConsulHealthChecksConfiguration(IApplicationLifetime lifetime, IHostingEnvironment environment, IConfiguration configuration)
        {
            _lifetime = lifetime;
            _environment = environment;
            _configuration = configuration.GetConsulConfiguration();
            _localPort = configuration.GetlocalPort();
        }

        public async Task ExecuteAsync(PathString path)
        {
            var localIpAdress = _configuration.CheckHealthHost ??
                                await _configuration.Host.GetLinkHostLocalIpAdresssAsync();
                               

            var localIpPort = _configuration.CheckHealthPort ?? _localPort;

            using (var _client = _configuration.MakeConsulClient())
            {
                var httpCheck = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(1),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(5),//健康检查时间间隔，或者称为心跳间隔
                    HTTP = $"{_configuration.CheckHealthScheme}://{localIpAdress}:{localIpPort}{path}",//健康检查地址
                    Timeout = TimeSpan.FromSeconds(_configuration.CheckHealthTimeout),
                };

                var registration = new AgentServiceRegistration()
                {
                    ID = $"{localIpAdress}:{_configuration.CheckHealthPort}".ToMd5String(),
                    Name = _environment.ApplicationName,
                    Checks = new[] { httpCheck },
                    Address = localIpAdress,
                    Port = localIpPort,
                    Tags = new[] { $"micSrv-/{_environment.ApplicationName}" }//添加 urlprefix-/servicename 格式的 tag 标签，以便 Fabio 识别
                };

                //服务启动时注册，内部实现其实就是使用 Consul API 进行注册（HttpClient发起）
                await _client.Agent.ServiceRegister(registration);
                _lifetime.ApplicationStopping.Register(() =>
                {
                    _client.Agent.ServiceDeregister(registration.ID).Wait();//服务停止时取消注册
                });
            }
        }

    }
}
