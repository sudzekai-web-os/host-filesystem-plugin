using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Application.Services.Filesystem
{
    [StructLayout(LayoutKind.Explicit, Size = 256)]
    internal partial struct LinuxFileStatus
    {
        private const uint _requiredFields = 0x1f;

        [FieldOffset(0)]
        private uint _mask;

        [FieldOffset(16)]
        private uint _linkCount;

        [FieldOffset(20)]
        public uint UserId;

        [FieldOffset(24)]
        public uint GroupId;

        [FieldOffset(28)]
        private ushort _mode;

        public readonly UnixFileMode Mode => (UnixFileMode)(_mode & 0xfff);

        public readonly bool IsRegularEntry
            => (_mode & 0xf000) is 0x4000 || ((_mode & 0xf000) == 0x8000 && _linkCount == 1);

        public static LinuxFileStatus Read(string path)
        {
            const int currentDirectory = -100;
            const int noFollow = 0x100;
            if (Statx(currentDirectory, path, noFollow, _requiredFields, out var result) != 0)
                throw new IOException("Could not read Linux file status", new Win32Exception(Marshal.GetLastPInvokeError()));

            if ((result._mask & _requiredFields) != _requiredFields)
                throw new IOException("Linux file status did not include the required fields");

            return result;
        }

        [LibraryImport("libc", EntryPoint = "statx", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        private static partial int Statx(int directory, string path, int flags, uint mask, out LinuxFileStatus result);
    }
}
