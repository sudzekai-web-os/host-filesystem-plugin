using Application.Dto;

namespace Application.Dispatchering.Commands.Directories
{
    public sealed record CreateDirectoryCommand(PathRequest Dto) : ICommand<DirectoryEntry>;
}
