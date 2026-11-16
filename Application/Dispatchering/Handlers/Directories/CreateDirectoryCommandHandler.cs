using Application.Dispatchering.Commands.Directories;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Directories
{
    internal sealed class CreateDirectoryCommandHandler(DirectoryService service, FilesystemLock filesystemLock) : ICommandHandler<CreateDirectoryCommand, DirectoryEntry>
    {
        public DirectoryEntry Handle(CreateDirectoryCommand command)
            => filesystemLock.Execute(() => service.CreateDirectory(command.Dto));
    }
}
