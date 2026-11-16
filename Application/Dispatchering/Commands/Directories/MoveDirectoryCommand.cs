using Application.Dto;

namespace Application.Dispatchering.Commands.Directories
{
    public sealed record MoveDirectoryCommand(TransferRequest Dto) : ICommand<DirectoryEntry>;
}
