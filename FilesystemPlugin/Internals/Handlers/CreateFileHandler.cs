using Application.Dispatchering.Commands.Files;
using Application.Dispatchering.Dispatchers;
using Application.Dto;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;
using System.Net;

namespace FilesystemPlugin
{
    internal sealed class CreateFileHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history, HttpStatusCode.Created)
    {
        public override string Pattern => "filesystem/files";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Post;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Создание файла", ["filesystem"], "Создание файла")
        {
            Description = "Создаёт файл по path. Поле content задаёт начальный текст, по умолчанию файл пустой.",
            RequestBody = new(FilesystemJsonContext.Default.TextRequest),
            Responses = [
                new(201, FilesystemJsonContext.Default.FileDetails),
                new(400), new(401), new(403), new(404), new(409), new(413), new(415), new(502), new(503)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<CreateFileCommand, FileDetails>(new(ReadBody(request, FilesystemJsonContext.Default.TextRequest)));
    }
}
