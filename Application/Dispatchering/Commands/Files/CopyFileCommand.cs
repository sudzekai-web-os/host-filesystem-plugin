using Application.Dto;

namespace Application.Dispatchering.Commands.Files
{
    public sealed record CopyFileCommand(TransferRequest Dto) : ICommand<FileDetails>;
}
