using Application.Dispatchering.Commands.Files;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class ReadTextCommandHandler(FileService service, FilesystemLock filesystemLock) : ICommandHandler<ReadTextCommand, string>
    {
        public string Handle(ReadTextCommand command)
            => filesystemLock.Execute(() => service.ReadText(command.Path));
    }
}
