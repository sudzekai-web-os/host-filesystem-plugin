using Application.Dispatchering.Commands.Files;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class WriteTextCommandHandler(FileService service, FilesystemLock filesystemLock) : ICommandHandler<WriteTextCommand, object?>
    {
        public object? Handle(WriteTextCommand command)
            => filesystemLock.Execute<object?>(() =>
            {
                service.WriteText(command.Dto);
                return null;
            });
    }
}
