using Application.Dispatchering.Commands.Files;
using Application.Dispatchering.Dispatchers;
using Application.Dto;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class ExportFileHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history)
    {
        public override string Pattern => "filesystem/files/export";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Get;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Экспорт файла", ["filesystem"], "Экспорт файла")
        {
            Description = "Возвращает содержимое файла в Base64. Лимит размера — 8 МиБ.",
            Parameters = [new("path", "query", FilesystemJsonContext.Default.String, "Путь в формате root:\\", required: true)],
            Responses = [new(200, FilesystemJsonContext.Default.String), new(400), new(401), new(403), new(404), new(409), new(413), new(415)]
        };

        protected override object Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<ExportFileCommand, string>(new(ReadPath(request)));

    }
}
