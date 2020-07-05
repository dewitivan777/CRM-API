using System.Net.Http;

namespace Client
{
    public interface IHttpClientFactory
    {
        HttpClient CreateHttpClient();
    }
}