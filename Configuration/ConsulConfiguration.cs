using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.Consul.AspNetCore.Configuration
{
    public class ConsulConfiguration
    {
        public string Scheme { get; set; } = "Http";
        public string Host { get; set; }
        public int Port { get; set; } = 8500;
        public string KVPrefix { get; set; } = "config";
        public string KVDataKey { get; set; } = "data";
        public string KVProfileSeparator { get; set; } = "-";
        public bool KVUploadDefault { get; set; } = true;
        public string CheckHealthScheme { get; set; } = "Http";
        public string CheckHealthHost { get; set; }
        public int? CheckHealthPort { get; set; }
        public double CheckHealthTimeout { get; set; } = 5d;
    }
}
