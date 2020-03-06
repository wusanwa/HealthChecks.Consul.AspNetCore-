using Consul;
using HealthChecks.Consul.AspNetCore;
using HealthChecks.Consul.AspNetCore.Configuration;
using HealthChecks.Consul.AspNetCore.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConsulHealthCheckServiceCollectionExtensions
    {
        public static IServiceCollection AddConsulHealthChecks(this IServiceCollection services)
        {
            services.AddTransient<IConsulHealthChecksConfiguration, ConsulHealthChecksConfiguration>();
            //services.AddSingleton<ConsulConfiguration>(sp => sp.GetRequiredService<IConfiguration>().GetConsulConfiguration());
            //services.AddSingleton<ConsulClient>(sp => sp.GetRequiredService<ConsulConfiguration>().MakeConsulClient());
            return services;
        }
        private static void UseConsulHealthChecksEndPoint(this IApplicationBuilder app, PathString path)
            => app.Map(path, c =>
                  c.Use(async (ctx, next) =>
                        await ctx.Response.WriteAsync("Healthy")));

        public static IApplicationBuilder UseConsulHealthChecks(this IApplicationBuilder app, PathString path)
        {
            app.UseConsulHealthChecksEndPoint(path);
            app.ApplicationServices.GetRequiredService<IConsulHealthChecksConfiguration>().ExecuteAsync(path).Wait();
            return app;
        }

    }
}
