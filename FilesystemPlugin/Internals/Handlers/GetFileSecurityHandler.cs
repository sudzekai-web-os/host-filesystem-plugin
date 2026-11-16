using Application.Dispatchering.Commands.Files;
using Application.Dispatchering.Dispatchers;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class GetFileSecurityHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history)
    {
        public override string Pattern => "filesystem/files/security";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Get;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Безопасность файла", ["filesystem"], "Безопасность файла")
        {
            Description = "На Windows возвращает владельца и правила ACL, на Linux — UID, GID и права POSIX.",
            Parameters = [new("path", "query", FilesystemJsonContext.Default.String, "Путь в формате root:\\", required: true)],
            Responses = [
                new(200) { Schema = GetSecuritySchema() },
                new(400), new(401), new(403), new(404), new(409)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<GetFileSecurityCommand, object>(new(ReadPath(request)));
    }
}
