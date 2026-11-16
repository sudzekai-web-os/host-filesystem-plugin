using Application.Dto;

namespace Application.Dispatchering.Commands.Directories
{
    public sealed record GetFilesCommand(string Path) : ICommand<FileDetails[]>;
}
