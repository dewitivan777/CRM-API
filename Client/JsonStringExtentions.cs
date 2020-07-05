using System;
using System.Diagnostics;
using System.Text.Json;

namespace Client
{
    public static class JsonStringExtentions
    {
        [DebuggerStepThrough]
        public static T ToTypedObject<T>(this string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var typed = JsonSerializer.Deserialize<T>(json, options);
            return typed;
        }

        [DebuggerStepThrough]
        public static string ToJsonString(this object @object)
        {
            if (@object is string) throw new ArgumentException("value can not be string", nameof(@object));

            var json = JsonSerializer.Serialize(@object);
            return json;
        }
    }
}