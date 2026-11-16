using Application.Dto;

namespace Application.Dispatchering.Commands.Files
{
    public sealed record DeleteFileCommand(PathRequest Dto) : ICommand<object?>;
}
