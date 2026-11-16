using Application.Dto;

namespace Application.Dispatchering.Commands.Directories
{
    public sealed record GetDirectoryDetailsCommand(string Path) : ICommand<DirectoryDetails>;
}
