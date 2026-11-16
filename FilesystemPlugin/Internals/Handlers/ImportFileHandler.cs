using Application.Dispatchering.Commands.Files;
using Application.Dispatchering.Dispatchers;
using Application.Dto;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class ImportFileHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history, System.Net.HttpStatusCode.Created)
    {
        public override string Pattern => "filesystem/files/import";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Post;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Импорт файла", ["filesystem"], "Импорт файла")
        {
            Description = "Создаёт файл из содержимого Base64. Существующий файл сохраняется. Лимит размера — 8 МиБ.",
            RequestBody = new(FilesystemJsonContext.Default.FileTransferRequest),
            Responses = [new(201, FilesystemJsonContext.Default.FileDetails), new(400), new(401), new(403), new(404), new(409), new(413), new(415)]
        };

        protected override object Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<ImportFileCommand, FileDetails>(new(ReadBody(request, FilesystemJsonContext.Default.FileTransferRequest, TransferRequestLimit)));

        protected override int BodySizeLimit => TransferRequestLimit;
    }
}
