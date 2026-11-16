using Application.Dispatchering.Commands.Directories;
using Application.Dispatchering.Dispatchers;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class GetDirectorySecurityHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history)
    {
        public override string Pattern => "filesystem/directories/security";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Get;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Безопасность директории", ["filesystem"], "Безопасность директории")
        {
            Description = "На Windows возвращает владельца и правила ACL, на Linux — UID, GID и права POSIX.",
            Parameters = [new("path", "query", FilesystemJsonContext.Default.String, "Путь в формате root:\\", required: true)],
            Responses = [
                new(200) { Schema = GetSecuritySchema() },
                new(400), new(401), new(403), new(404), new(409)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<GetDirectorySecurityCommand, object>(new(ReadPath(request)));
    }
}
