using Application.Dto;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Application.Services.Filesystem
{
    internal static class FilesystemSecurity
    {
        public static object Read(FileSystemInfo entry)
        {
            if (OperatingSystem.IsWindows())
                return ReadWindows(entry);
            if (OperatingSystem.IsLinux())
                return ReadLinux(entry.FullName);

            throw new PlatformNotSupportedException();
        }

        [SupportedOSPlatform("windows")]
        private static WindowsSecurityInfo ReadWindows(FileSystemInfo entry)
        {
            const AccessControlSections sections = AccessControlSections.Owner | AccessControlSections.Group | AccessControlSections.Access;
            FileSystemSecurity security = entry is DirectoryInfo directory
                ? directory.GetAccessControl(sections)
                : ((FileInfo)entry).GetAccessControl(sections);
            var rules = security.GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier))
                .OfType<FileSystemAccessRule>()
                .Select(rule => new WindowsAccessRule(rule.IdentityReference.Value, rule.FileSystemRights.ToString(),
                    rule.AccessControlType.ToString(), rule.IsInherited, rule.InheritanceFlags.ToString(), rule.PropagationFlags.ToString()))
                .ToArray();

            return new("windows", security.GetOwner(typeof(SecurityIdentifier))?.Value, security.GetGroup(typeof(SecurityIdentifier))?.Value,
                security.AreAccessRulesProtected, rules);
        }

        private static LinuxSecurityInfo ReadLinux(string path)
        {
            var status = LinuxFileStatus.Read(path);
            var mode = (int)status.Mode;
            return new("linux", status.UserId, status.GroupId, Convert.ToString(mode, 8).PadLeft(4, '0'),
                ToPermissions(mode >> 6), ToPermissions(mode >> 3), ToPermissions(mode));
        }

        private static string ToPermissions(int bits)
            => string.Create(3, bits, static (text, permissions) =>
            {
                text[0] = (permissions & 4) != 0 ? 'r' : '-';
                text[1] = (permissions & 2) != 0 ? 'w' : '-';
                text[2] = (permissions & 1) != 0 ? 'x' : '-';
            });
    }
}
