using Application.Dispatchering.Commands.Directories;
using Application.Dispatchering.Dispatchers;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class DeleteDirectoryHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history)
    {
        public override string Pattern => "filesystem/directories";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Delete;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Удаление директории", ["filesystem"], "Удаление директории")
        {
            Description = "Удаляет директорию по path. Для удаления с содержимым требуется recursive: true.",
            RequestBody = new(FilesystemJsonContext.Default.DirectoryDeleteRequest),
            Responses = [
                new(200) { Schema = """{"type":"null"}""" },
                new(400), new(401), new(403), new(404), new(409), new(413), new(415), new(502), new(503)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<DeleteDirectoryCommand, object?>(new(ReadBody(request, FilesystemJsonContext.Default.DirectoryDeleteRequest)));
    }
}
