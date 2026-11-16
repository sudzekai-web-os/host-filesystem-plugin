using Application.Dispatchering.Commands.Files;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class MoveFileCommandHandler(FileService service, FilesystemLock filesystemLock) : ICommandHandler<MoveFileCommand, FileDetails>
    {
        public FileDetails Handle(MoveFileCommand command)
            => filesystemLock.Execute(() => service.MoveFile(command.Dto));
    }
}
