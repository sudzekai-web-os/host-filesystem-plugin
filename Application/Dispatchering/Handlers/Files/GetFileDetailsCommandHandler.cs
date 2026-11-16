using Application.Dispatchering.Commands.Files;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class GetFileDetailsCommandHandler(FileService service, FilesystemLock filesystemLock) : ICommandHandler<GetFileDetailsCommand, FileDetails>
    {
        public FileDetails Handle(GetFileDetailsCommand command)
            => filesystemLock.Execute(() => service.GetFileDetails(command.Path));
    }
}
