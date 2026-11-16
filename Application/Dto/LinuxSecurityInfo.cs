namespace Application.Dto
{
    public sealed record LinuxSecurityInfo(string Platform, uint UserId, uint GroupId, string Mode,
        string OwnerPermissions, string GroupPermissions, string OtherPermissions);
}
