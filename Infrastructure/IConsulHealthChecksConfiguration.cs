using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HealthChecks.Consul.AspNetCore.Infrastructure
{
    internal interface IConsulHealthChecksConfiguration
    {
        Task ExecuteAsync(PathString path);
    }
}