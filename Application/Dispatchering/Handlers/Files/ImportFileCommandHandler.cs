using Application.Dispatchering.Commands.Files;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class ImportFileCommandHandler(FileService files, FilesystemLock gate) : ICommandHandler<ImportFileCommand, FileDetails>
    {
        public FileDetails Handle(ImportFileCommand command)
                => gate.Execute(() => files.ImportFile(command.Request));
    }
}
