namespace Application.Dto
{
    public sealed record WindowsAccessRule(string IdentitySid, string Rights, string AccessType, bool IsInherited,
        string InheritanceFlags, string PropagationFlags);
}
