using System.Threading.Tasks;

namespace ServiceRegistry
{
    public interface IServiceRegistry
    {
        bool DeRegisterService(string id);
        Task<string> GetServiceLocation(string name, string operation);
        Task<string> GetValue(string key);
    }
}
