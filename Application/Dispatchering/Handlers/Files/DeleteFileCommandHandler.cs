using Application.Dispatchering.Commands.Files;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class DeleteFileCommandHandler(FileService service, FilesystemLock filesystemLock) : ICommandHandler<DeleteFileCommand, object?>
    {
        public object? Handle(DeleteFileCommand command)
            => filesystemLock.Execute<object?>(() =>
            {
                service.DeleteFile(command.Dto);
                return null;
            });
    }
}
