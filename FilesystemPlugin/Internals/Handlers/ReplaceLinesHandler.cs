using Application.Dispatchering.Commands.Files;
using Application.Dispatchering.Dispatchers;
using Application.Services.Database;
using Sudzekai.LinCtrl.Abi.Managed;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    internal sealed class ReplaceLinesHandler(IDispatcher dispatcher, DatabaseHistoryService history)
        : FilesystemRequestHandler(dispatcher, history)
    {
        public override string Pattern => "filesystem/files/content";

        public override HttpRequestMethod HttpMethod => HttpRequestMethod.Patch;

        public override HandlerMetadata Metadata { get; } = new(
            string.Empty, "Замена строк", ["filesystem"], "Замена строк")
        {
            Description = "Заменяет строки файла по path. В словаре lines ключ — номер строки начиная с 1, значение — новый текст.",
            RequestBody = new(FilesystemJsonContext.Default.ReplaceLinesRequest),
            Responses = [
                new(200) { Schema = """{"type":"null"}""" },
                new(400), new(401), new(403), new(404), new(409), new(413), new(415), new(502), new(503)
            ]
        };

        protected override object? Execute(IDispatcher dispatcher, HttpRequestDto request)
            => dispatcher.Execute<ReplaceLinesCommand, object?>(new(ReadBody(request, FilesystemJsonContext.Default.ReplaceLinesRequest)));
    }
}
