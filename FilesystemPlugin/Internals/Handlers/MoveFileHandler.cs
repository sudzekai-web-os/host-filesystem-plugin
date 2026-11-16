using Application.Dispatchering.Commands.Files;
using Application.Dispatchering.Dispatchers;
using Application.Dto;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class MoveFileHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history)
    {
        public override string Pattern => "filesystem/files/move";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Post;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Перемещение файла", ["filesystem"], "Перемещение файла")
        {
            Description = "Перемещает файл из source в новый путь destination.",
            RequestBody = new(FilesystemJsonContext.Default.TransferRequest),
            Responses = [
                new(200, FilesystemJsonContext.Default.FileDetails),
                new(400), new(401), new(403), new(404), new(409), new(413), new(415), new(502), new(503)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<MoveFileCommand, FileDetails>(new(ReadBody(request, FilesystemJsonContext.Default.TransferRequest)));
    }
}
