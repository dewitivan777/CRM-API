using Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client
{
    public class NameResolver : INameResolver
    {
        private readonly IApiClient _apiClient;
        private readonly Dictionary<string, RoutingInfo> _knownNames;

        private readonly static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public NameResolver(IApiClient apiClient)
        {
            _apiClient = apiClient;
            _knownNames = new Dictionary<string, RoutingInfo>(StringComparer.OrdinalIgnoreCase)
            {
                ["category"] = new RoutingInfo("classification", "query", "/service/classification/category"),
                ["city"] = new RoutingInfo("classification", "query", "/service/classification/city"),
                ["component"] = new RoutingInfo("classification", "query", "/service/classification/component"),
                ["componentMake"] = new RoutingInfo("classification", "query", "/service/classification/componentmake"),
                //_knownNames["ContactTypeId"] = new RoutingInfo("classification", "query", "/service/classification/contacttype");
                ["country"] = new RoutingInfo("classification", "query", "/service/classification/country"),
                ["group"] = new RoutingInfo("classification", "query", "/service/classification/group"),
                //_knownNames["IntentionId"] = new RoutingInfo("classification", "query", "/service/classification/intention");
                //_knownNames["LeadTypeId"] = new RoutingInfo("classification", "query", "/service/classification/leadtype");
                //_knownNames["NotificationTypeId"] = new RoutingInfo("classification", "query", "/service/classification/notificationtype");
                //_knownNames["PackageId"] = new RoutingInfo("classification", "query", "/service/classification/package");
                ["region"] = new RoutingInfo("classification", "query", "/service/classification/region"),
                //_knownNames["SettingId"] = new RoutingInfo("classification", "query", "/service/classification/setting");
                //_knownNames["SourceId"] = new RoutingInfo("classification", "query", "/service/classification/source");
                //_knownNames["StateId"] = new RoutingInfo("classification", "query", "/service/classification/state");
                ["suburb"] = new RoutingInfo("classification", "query", "/service/classification/suburb")
            };
        }

        public async Task<string> ResolveNameAsync(string entityId, string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                return null;

            if (entityType.EndsWith("Id"))
            {
                entityType = entityType.Remove(entityType.Length - 2);
            }

            var serviceName = entityType.ToLowerInvariant();

            var serviceQuery = await _apiClient.GetAsync<ApiNameModel>(
                serviceInfo: new ApiInfo("classification", method: "query"),
                pathWithQuery: $"/service/classification/{serviceName}/{entityId}");

            if (!serviceQuery.IsError && serviceQuery.Content != null)
            {
                return serviceQuery.Content.Name;
            }

            return null;
        }

        public Task<string> ResolveNameAsync(int entityId, string entityType)
        {
            return ResolveNameAsync(entityId.ToString(), entityType);
        }

        public async Task<T> ResolveNamesAsync<T>(T entity) where T : class
        {
            string json = JsonSerializer.Serialize(entity, _jsonOptions);

            using var stream = new MemoryStream();
            using (var jsonWriter = new Utf8JsonWriter(stream))
            {
                using (JsonDocument jsonDocument = JsonDocument.Parse(json))
                {
                    jsonWriter.WriteStartObject();

                    foreach (var element in jsonDocument.RootElement.EnumerateObject())
                    {
                        if (_knownNames.ContainsKey(element.Name))
                        {
                            var identifierFieldName = element.Name + "Id";
                            if (jsonDocument.RootElement.TryGetProperty(identifierFieldName, out var identifierFieldValue))
                            {
                                if (!identifierFieldValue.Equals(default))
                                {
                                    string itemValue = $"{identifierFieldValue}";

                                    if (!string.IsNullOrWhiteSpace(itemValue))
                                    {
                                        var serviceRoute = _knownNames[element.Name];

                                        var serviceQuery = await _apiClient.GetAsync<ApiNameModel>(
                                            serviceInfo: new ApiInfo(serviceRoute.Service, method: serviceRoute.Method),
                                            pathWithQuery: $"{serviceRoute.Route}/{itemValue}");

                                        if (!serviceQuery.IsError && serviceQuery.Content != null)
                                        {
                                            jsonWriter.WriteString(element.Name, serviceQuery.Content.Name);
                                        }
                                    }
                                    else
                                    {
                                        // should we clear it?
                                        jsonWriter.WriteNull(element.Name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            element.WriteTo(jsonWriter);
                        }
                    }
                }
                jsonWriter.WriteEndObject();
            }

            json = Encoding.UTF8.GetString(stream.ToArray());

            var result = JsonSerializer.Deserialize<T>(json, _jsonOptions);

            return result;
        }
    }
}
