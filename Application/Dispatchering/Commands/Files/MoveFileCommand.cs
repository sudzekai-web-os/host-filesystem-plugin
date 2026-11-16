using Application.Dto;

namespace Application.Dispatchering.Commands.Files
{
    public sealed record MoveFileCommand(TransferRequest Dto) : ICommand<FileDetails>;
}
