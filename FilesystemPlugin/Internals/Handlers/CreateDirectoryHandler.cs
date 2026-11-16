using Application.Dispatchering.Commands.Directories;
using Application.Dispatchering.Dispatchers;
using Application.Dto;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;
using System.Net;

namespace FilesystemPlugin
{
    internal sealed class CreateDirectoryHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history, HttpStatusCode.Created)
    {
        public override string Pattern => "filesystem/directories";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Post;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Создание директории", ["filesystem"], "Создание директории")
        {
            Description = "Создаёт директорию по пути path внутри root:. Родительская директория должна существовать.",
            RequestBody = new(FilesystemJsonContext.Default.PathRequest),
            Responses = [
                new(201, FilesystemJsonContext.Default.DirectoryEntry),
                new(400), new(401), new(403), new(404), new(409), new(413), new(415), new(502), new(503)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<CreateDirectoryCommand, DirectoryEntry>(new(ReadBody(request, FilesystemJsonContext.Default.PathRequest)));
    }
}
