using Application.Dispatchering.Commands.Directories;
using Application.Dispatchering.Dispatchers;
using Application.Dto;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class GetDirectoryDetailsHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history)
    {
        public override string Pattern => "filesystem/directories/info";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Get;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Информация о директории", ["filesystem"], "Информация о директории")
        {
            Description = "Возвращает атрибуты, размер и количество вложенных объектов. Время указано в UTC.",
            Parameters = [new("path", "query", FilesystemJsonContext.Default.String, "Путь в формате root:\\", required: true)],
            Responses = [
                new(200, FilesystemJsonContext.Default.DirectoryDetails),
                new(400), new(401), new(403), new(404), new(409)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<GetDirectoryDetailsCommand, DirectoryDetails>(new(ReadPath(request)));
    }
}
