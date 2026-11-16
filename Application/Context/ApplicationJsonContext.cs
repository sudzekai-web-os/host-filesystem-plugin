using Application.Dto;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Application.Context
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(HistoryEntry))]
    [JsonSerializable(typeof(LoginRequest))]
    [JsonSerializable(typeof(LoginResponse))]
    internal partial class ApplicationJsonContext : JsonSerializerContext
    {
        public JsonTypeInfo<T> GetTypeInfo<T>()
            => (JsonTypeInfo<T>)GetTypeInfo(typeof(T))!;
    }
}