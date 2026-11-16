using Application.Dispatchering.Commands.Files;
using Application.Dispatchering.Dispatchers;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class DeleteFileHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history)
    {
        public override string Pattern => "filesystem/files";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Delete;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Удаление файла", ["filesystem"], "Удаление файла")
        {
            Description = "Удаляет файл по пути path внутри root:.",
            RequestBody = new(FilesystemJsonContext.Default.PathRequest),
            Responses = [
                new(200) { Schema = """{"type":"null"}""" },
                new(400), new(401), new(403), new(404), new(409), new(413), new(415), new(502), new(503)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<DeleteFileCommand, object?>(new(ReadBody(request, FilesystemJsonContext.Default.PathRequest)));
    }
}
