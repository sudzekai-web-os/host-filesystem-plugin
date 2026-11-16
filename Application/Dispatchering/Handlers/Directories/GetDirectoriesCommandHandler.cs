using Application.Dispatchering.Commands.Directories;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Directories
{
    internal sealed class GetDirectoriesCommandHandler(DirectoryService service, FilesystemLock filesystemLock) : ICommandHandler<GetDirectoriesCommand, DirectoryEntry[]>
    {
        public DirectoryEntry[] Handle(GetDirectoriesCommand command)
            => filesystemLock.Execute(() => service.GetDirectories(command.Path));
    }
}
