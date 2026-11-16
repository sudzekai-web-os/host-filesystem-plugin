using Application.Dispatchering.Commands.Directories;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Directories
{
    internal sealed class DeleteDirectoryCommandHandler(DirectoryService service, FilesystemLock filesystemLock) : ICommandHandler<DeleteDirectoryCommand, object?>
    {
        public object? Handle(DeleteDirectoryCommand command)
            => filesystemLock.Execute<object?>(() =>
            {
                service.DeleteDirectory(command.Dto);
                return null;
            });
    }
}
