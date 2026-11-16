using Application.Dto;

namespace Application.Dispatchering.Commands.Files
{
    public sealed record GetFileDetailsCommand(string Path) : ICommand<FileDetails>;
}
