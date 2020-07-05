using System;
using System.Collections.Generic;

namespace ServiceRegistry
{
    public class ServiceDefinition : IEquatable<ServiceDefinition>
    {
        public string Address { get; set; }
        public ServiceCheck Check { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Operations { get; set; }

        public int Port { get; set; }

        public string Protocol { get; set; }

        public bool Equals(ServiceDefinition other)
        {
            if (other is null || ReferenceEquals(this, other)) return true;

            return Name == other.Name
                && Port == other.Port
                && Address == other.Address;
        }

        public override bool Equals(object obj) => Equals(obj as ServiceDefinition);
        public static bool operator ==(ServiceDefinition lhs, ServiceDefinition rhs) => Equals(lhs, rhs);
        public static bool operator !=(ServiceDefinition lhs, ServiceDefinition rhs) => !(lhs == rhs);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Name?.GetHashCode() ?? 0;
                hash = hash * 23 + Address?.GetHashCode() ?? 0;
                hash = hash * 23 + Port.GetHashCode();
                return hash;
            }
        }
    }
}
