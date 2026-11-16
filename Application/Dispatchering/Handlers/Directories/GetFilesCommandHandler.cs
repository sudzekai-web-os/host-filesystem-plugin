using Application.Dispatchering.Commands.Directories;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Directories
{
    internal sealed class GetFilesCommandHandler(DirectoryService service, FilesystemLock filesystemLock) : ICommandHandler<GetFilesCommand, FileDetails[]>
    {
        public FileDetails[] Handle(GetFilesCommand command)
            => filesystemLock.Execute(() => service.GetFiles(command.Path));
    }
}
