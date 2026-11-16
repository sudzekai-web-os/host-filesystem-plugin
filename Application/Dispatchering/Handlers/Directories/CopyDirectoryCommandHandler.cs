using Application.Dispatchering.Commands.Directories;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Directories
{
    internal sealed class CopyDirectoryCommandHandler(DirectoryService service, FilesystemLock filesystemLock) : ICommandHandler<CopyDirectoryCommand, DirectoryEntry>
    {
        public DirectoryEntry Handle(CopyDirectoryCommand command)
            => filesystemLock.Execute(() => service.CopyDirectory(command.Dto));
    }
}
