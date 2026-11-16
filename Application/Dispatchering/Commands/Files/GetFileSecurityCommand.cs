namespace Application.Dispatchering.Commands.Files
{
    public sealed record GetFileSecurityCommand(string Path) : ICommand<object>;
}
