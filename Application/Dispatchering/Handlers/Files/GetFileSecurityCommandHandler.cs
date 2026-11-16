using Application.Dispatchering.Commands.Files;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class GetFileSecurityCommandHandler(FileService service, FilesystemLock filesystemLock) : ICommandHandler<GetFileSecurityCommand, object>
    {
        public object Handle(GetFileSecurityCommand command)
            => filesystemLock.Execute(() => service.GetFileSecurity(command.Path));
    }
}
