using Application.Dispatchering.Commands.Directories;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Directories
{
    internal sealed class MoveDirectoryCommandHandler(DirectoryService service, FilesystemLock filesystemLock) : ICommandHandler<MoveDirectoryCommand, DirectoryEntry>
    {
        public DirectoryEntry Handle(MoveDirectoryCommand command)
            => filesystemLock.Execute(() => service.MoveDirectory(command.Dto));
    }
}
