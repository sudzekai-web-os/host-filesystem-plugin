using Application.Dto;

namespace Application.Dispatchering.Commands.Directories
{
    public sealed record GetDirectoriesCommand(string Path) : ICommand<DirectoryEntry[]>;
}
