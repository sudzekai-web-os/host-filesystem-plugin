using Application.Dto;
using System.Text.Json.Serialization;

namespace FilesystemPlugin
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(DirectoryEntry))]
    [JsonSerializable(typeof(DirectoryEntry[]))]
    [JsonSerializable(typeof(DirectoryDetails))]
    [JsonSerializable(typeof(FileDetails))]
    [JsonSerializable(typeof(FileDetails[]))]
    [JsonSerializable(typeof(WindowsSecurityInfo))]
    [JsonSerializable(typeof(LinuxSecurityInfo))]
    [JsonSerializable(typeof(PathRequest))]
    [JsonSerializable(typeof(DirectoryDeleteRequest))]
    [JsonSerializable(typeof(TransferRequest))]
    [JsonSerializable(typeof(TextRequest))]
    [JsonSerializable(typeof(FileTransferRequest))]
    [JsonSerializable(typeof(ReplaceLinesRequest))]
    internal partial class FilesystemJsonContext : JsonSerializerContext
    {
    }
}
