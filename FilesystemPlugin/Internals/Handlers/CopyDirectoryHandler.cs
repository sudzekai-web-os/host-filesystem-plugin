using Application.Dispatchering.Commands.Directories;
using Application.Dispatchering.Dispatchers;
using Application.Dto;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;
using System.Net;

namespace FilesystemPlugin
{
    internal sealed class CopyDirectoryHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history, HttpStatusCode.Created)
    {
        public override string Pattern => "filesystem/directories/copy";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Post;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Копирование директории", ["filesystem"], "Копирование директории")
        {
            Description = "Копирует source со всем содержимым в новый путь destination.",
            RequestBody = new(FilesystemJsonContext.Default.TransferRequest),
            Responses = [
                new(201, FilesystemJsonContext.Default.DirectoryEntry),
                new(400), new(401), new(403), new(404), new(409), new(413), new(415), new(502), new(503)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<CopyDirectoryCommand, DirectoryEntry>(new(ReadBody(request, FilesystemJsonContext.Default.TransferRequest)));
    }
}
