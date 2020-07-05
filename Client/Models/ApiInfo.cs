using System;

namespace Client.Models
{
    public class ApiInfo
    {
        public ApiInfo(string name, string method)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(method)) throw new ArgumentNullException(nameof(name));

            Name = name;
            Method = method;
        }
        public string Name { get; }
        public string Method { get; }
    }
}