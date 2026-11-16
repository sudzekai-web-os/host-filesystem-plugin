using Application.Dispatchering.Commands.Files;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class CopyFileCommandHandler(FileService service, FilesystemLock filesystemLock) : ICommandHandler<CopyFileCommand, FileDetails>
    {
        public FileDetails Handle(CopyFileCommand command)
            => filesystemLock.Execute(() => service.CopyFile(command.Dto));
    }
}
