using Application.Dispatchering.Commands.Directories;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Directories
{
    internal sealed class GetDirectorySecurityCommandHandler(DirectoryService service, FilesystemLock filesystemLock) : ICommandHandler<GetDirectorySecurityCommand, object>
    {
        public object Handle(GetDirectorySecurityCommand command)
            => filesystemLock.Execute(() => service.GetDirectorySecurity(command.Path));
    }
}
