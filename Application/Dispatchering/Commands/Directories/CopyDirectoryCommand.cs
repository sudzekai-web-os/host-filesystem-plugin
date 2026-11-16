using Application.Dto;

namespace Application.Dispatchering.Commands.Directories
{
    public sealed record CopyDirectoryCommand(TransferRequest Dto) : ICommand<DirectoryEntry>;
}
