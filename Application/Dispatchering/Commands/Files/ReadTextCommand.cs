namespace Application.Dispatchering.Commands.Files
{
    public sealed record ReadTextCommand(string Path) : ICommand<string>;
}
