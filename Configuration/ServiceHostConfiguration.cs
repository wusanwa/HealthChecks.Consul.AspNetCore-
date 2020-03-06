namespace HealthChecks.Consul.AspNetCore.Configuration
{
    public class ServiceHostConfiguration
    {
        public string Scheme { get; set; } = "Http";
        public string Host { get; set; }
        public int Port { get; set; } = 80;
    }
}
