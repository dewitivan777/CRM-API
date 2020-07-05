using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceRegistry
{
    /// <summary>
    /// Mem service registry
    /// </summary>
    public class MemoryServiceRegistry : IServiceRegistry, IDisposable
    {
        // Flag: Has Dispose already been called?
        private bool disposed = false;

        private readonly HttpClient _httpClient;
        private readonly IDisposable _changeToken;
        private IDictionary<string, HashSet<ServiceDefinition>> _serviceRegistry;

        public MemoryServiceRegistry(HttpClient httpClient, IOptionsMonitor<List<ServiceDefinition>> serviceListAccessor)
        {
            _httpClient = httpClient;
            RegisterServices(serviceListAccessor.CurrentValue);
            _changeToken = serviceListAccessor.OnChange(listener: RegisterServices);
        }


        public bool DeRegisterService(string id)
        {
            return true;
        }

        public async Task<string> GetServiceLocation(string name, string operation)
        {
            if (name.ToLower().EndsWith("advert"))
                name = "advert";

            var serviceEntryKey = string.Concat(name, operation);

            if (_serviceRegistry.TryGetValue(serviceEntryKey, out var serviceInstances))
            {
                if (serviceInstances?.Count > 0)
                {
                    // health check only if more than one instances were registered
                    if (serviceInstances.Count > 1)
                    {
                        // return first passing instance
                        foreach (var instance in serviceInstances.Where(s => s.Check != null).OrderBy(s => s.Check.Timeout))
                        {
                            try
                            {
                                using (var source = new CancellationTokenSource(instance.Check.Timeout))
                                {
                                    var health = await _httpClient.GetAsync(string.Concat(instance.Address, instance.Check.Http), source.Token);
                                    if (health.IsSuccessStatusCode)
                                    {
                                        return instance.Address;
                                    }
                                }
                            }
                            catch (Exception /* ex */) { }
                        }
                    }
                    else
                    {
                        var instance = serviceInstances.First();
                        return instance.Address;
                    }
                }
            }

            return null;
        }

        public Task<string> GetValue(string key)
        {
            throw new NotImplementedException();
        }


        private void RegisterServices(IList<ServiceDefinition> services)
        {
            var serviceRegistry
                = new Dictionary<string, HashSet<ServiceDefinition>>(StringComparer.OrdinalIgnoreCase);

            if (services != null)
            {
                foreach (var s in services)
                {
                    foreach (var op in s.Operations)
                    {
                        var serviceEntryKey = string.Concat(s.Name, op);

                        if (!serviceRegistry.ContainsKey(serviceEntryKey))
                        {
                            serviceRegistry.Add(serviceEntryKey, new HashSet<ServiceDefinition>());
                        }

                        var address = s.Address;
                        var protocol = s.Protocol;
                        if (string.IsNullOrWhiteSpace(s.Address))
                        {
                            address = "127.0.0.1";
                        }

                        if (string.IsNullOrWhiteSpace(protocol))
                        {
                            protocol = "http";
                        }

                        address = string.Concat(protocol, "://", address, ":", s.Port);

                        serviceRegistry[serviceEntryKey].Add(new ServiceDefinition
                        {
                            Address = address,
                            Check = s.Check,
                            Name = s.Name,
                            Port = s.Port
                        });
                    }
                }
            }
            _serviceRegistry = serviceRegistry;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (_changeToken != null)
                    _changeToken.Dispose();
            }

            disposed = true;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        ~MemoryServiceRegistry()
        {
            Dispose(false);
        }
    }
}
