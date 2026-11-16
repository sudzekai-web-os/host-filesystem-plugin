using Application.Dispatchering.Commands.Directories;
using Application.Dispatchering.Dispatchers;
using Application.Dto;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class MoveDirectoryHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history)
    {
        public override string Pattern => "filesystem/directories/move";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Post;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Перемещение директории", ["filesystem"], "Перемещение директории")
        {
            Description = "Перемещает source со всем содержимым в новый путь destination.",
            RequestBody = new(FilesystemJsonContext.Default.TransferRequest),
            Responses = [
                new(200, FilesystemJsonContext.Default.DirectoryEntry),
                new(400), new(401), new(403), new(404), new(409), new(413), new(415), new(502), new(503)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<MoveDirectoryCommand, DirectoryEntry>(new(ReadBody(request, FilesystemJsonContext.Default.TransferRequest)));
    }
}
