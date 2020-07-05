using System.Net.Http;
using System.Net.Http.Headers;

namespace Client
{
    public class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            SetupClientDefaults(client);
            return client;
        }

        protected void SetupClientDefaults(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}