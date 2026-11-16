namespace Application.Dto
{
    public sealed record WindowsSecurityInfo(string Platform, string? OwnerSid, string? GroupSid,
        bool IsInheritanceProtected, WindowsAccessRule[] Rules);
}
