using Application.Dto;

namespace Application.Dispatchering.Commands.Files
{
    public sealed record ReplaceLinesCommand(ReplaceLinesRequest Dto) : ICommand<object?>;
}
