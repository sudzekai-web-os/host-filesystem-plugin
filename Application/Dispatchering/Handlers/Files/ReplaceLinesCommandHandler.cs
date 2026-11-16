using Application.Dispatchering.Commands.Files;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class ReplaceLinesCommandHandler(FileService service, FilesystemLock filesystemLock) : ICommandHandler<ReplaceLinesCommand, object?>
    {
        public object? Handle(ReplaceLinesCommand command)
            => filesystemLock.Execute<object?>(() =>
            {
                service.ReplaceLines(command.Dto);
                return null;
            });
    }
}
