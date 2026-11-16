using Application.Dto;

namespace Application.Dispatchering.Commands.Files
{
    public sealed record CreateFileCommand(TextRequest Dto) : ICommand<FileDetails>;
}
