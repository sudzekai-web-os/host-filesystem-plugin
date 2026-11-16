using Application.Dispatchering.Commands.Files;
using Application.Dto;
using Application.Services.Filesystem;

namespace Application.Dispatchering.Handlers.Files
{
    internal sealed class ExportFileCommandHandler(FileService files, FilesystemLock gate) : ICommandHandler<ExportFileCommand, string>
    {
        public string Handle(ExportFileCommand command)
                => gate.Execute(() => files.ExportFile(command.Path));
    }
}
