using Application.Dispatchering.Commands.Files;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class CreateFileCommandHandler(FileService service, FilesystemLock filesystemLock) : ICommandHandler<CreateFileCommand, FileDetails>
    {
        public FileDetails Handle(CreateFileCommand command)
            => filesystemLock.Execute(() => service.CreateFile(command.Dto));
    }
}
