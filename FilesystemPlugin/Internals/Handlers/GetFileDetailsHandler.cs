using Application.Dispatchering.Commands.Files;
using Application.Dispatchering.Dispatchers;
using Application.Dto;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class GetFileDetailsHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history)
    {
        public override string Pattern => "filesystem/files/info";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Get;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Информация о файле", ["filesystem"], "Информация о файле")
        {
            Description = "Возвращает имя, расширение, размер, атрибуты и время в UTC.",
            Parameters = [new("path", "query", FilesystemJsonContext.Default.String, "Путь в формате root:\\", required: true)],
            Responses = [
                new(200, FilesystemJsonContext.Default.FileDetails),
                new(400), new(401), new(403), new(404), new(409)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<GetFileDetailsCommand, FileDetails>(new(ReadPath(request)));
    }
}
