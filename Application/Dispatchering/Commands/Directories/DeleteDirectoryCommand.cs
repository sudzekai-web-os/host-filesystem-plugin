using Application.Dto;

namespace Application.Dispatchering.Commands.Directories
{
    public sealed record DeleteDirectoryCommand(DirectoryDeleteRequest Dto) : ICommand<object?>;
}
