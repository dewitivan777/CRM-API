using System;

namespace ServiceRegistry
{
    public class ServiceCheck
    {
        public string Http { get; set; }
        public TimeSpan Timeout { get; set; }
            = TimeSpan.FromSeconds(5);
    }
}
