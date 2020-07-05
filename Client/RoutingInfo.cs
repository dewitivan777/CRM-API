namespace Client
{
    public class RoutingInfo
    {
        public RoutingInfo(string service, string method, string route)
        {
            Service = service;
            Method = method;
            Route = route;
        }

        public string Service { get; set; }
        public string Method { get; set; }
        public string Route { get; set; }
    }
}
