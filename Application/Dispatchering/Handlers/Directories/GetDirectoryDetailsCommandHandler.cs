using Application.Dispatchering.Commands.Directories;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Directories
{
    internal sealed class GetDirectoryDetailsCommandHandler(DirectoryService service, FilesystemLock filesystemLock) : ICommandHandler<GetDirectoryDetailsCommand, DirectoryDetails>
    {
        public DirectoryDetails Handle(GetDirectoryDetailsCommand command)
            => filesystemLock.Execute(() => service.GetDirectoryDetails(command.Path));
    }
}
