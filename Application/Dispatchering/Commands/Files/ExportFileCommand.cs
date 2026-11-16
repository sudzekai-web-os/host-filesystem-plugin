using Application.Dto;

namespace Application.Dispatchering.Commands.Files
{
    public sealed record ExportFileCommand(string Path) : ICommand<string>;
}
