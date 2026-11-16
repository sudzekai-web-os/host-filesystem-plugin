namespace Application.Dispatchering.Commands.Directories
{
    public sealed record GetDirectorySecurityCommand(string Path) : ICommand<object>;
}
