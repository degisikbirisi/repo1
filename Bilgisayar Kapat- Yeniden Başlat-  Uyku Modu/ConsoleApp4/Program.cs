using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

    [DllImport("kernel32.dll")]
    static extern uint GetLastError();

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LUID
    {
        public uint LowPart;
        public uint HighPart;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TOKEN_PRIVILEGES
    {
        public uint PrivilegeCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public LUID_AND_ATTRIBUTES[] Privileges;
    }

    const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
    const uint TOKEN_QUERY = 0x0008;
    const uint SE_PRIVILEGE_ENABLED = 0x00000002;
    const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
    const uint EWX_LOGOFF = 0x00000000;
    const uint EWX_SHUTDOWN = 0x00000001;
    const uint EWX_REBOOT = 0x00000002;
    const uint EWX_FORCE = 0x00000004;
    const uint EWX_POWEROFF = 0x00000008;
    const uint EWX_FORCEIFHUNG = 0x00000010;

    static void Main(string[] args)
    {
        // Bilgisayarı kapat
        // Shutdown();

        // Bilgisayarı yeniden başlat
        // Reboot();

        // Bilgisayarı uyku moduna al
        // Sleep();
    }

    static void Shutdown()
    {
        if (!SetShutdownPrivilege(true))
        {
            return;
        }

        uint uFlags = EWX_SHUTDOWN | EWX_FORCE;
        uint dwReason = 0;

        if (!ExitWindowsEx(uFlags, dwReason))
        {
            uint dwError = GetLastError();
        }
    }

    static void Reboot()
    {
        if (!SetShutdownPrivilege(true))
        {
            return;
        }

        uint uFlags = EWX_REBOOT | EWX_FORCE;
        uint dwReason = 0;

        if (!ExitWindowsEx(uFlags, dwReason))
        {
            uint dwError = GetLastError();
        }
    }

    static void Sleep()
    {
        if (!SetShutdownPrivilege(true))
        {
            return;
        }

        uint uFlags = EWX_LOGOFF | EWX_FORCE;
        uint dwReason = 0;

        if (!ExitWindowsEx(uFlags, dwReason))
        {
            uint dwError = GetLastError();
        }
    }

    static bool SetShutdownPrivilege(bool enable)
    {
        IntPtr hToken;
        if (!OpenProcessToken(System.Diagnostics.Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out hToken))
        {
            return false;
        }

        LUID luid;
        if (!LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, out luid))
        {
            return false;
        }

        TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES();
        tp.PrivilegeCount = 1;
        tp.Privileges = new LUID_AND_ATTRIBUTES[1];
        tp.Privileges[0].Luid = luid;
        tp.Privileges[0].Attributes = enable ? SE_PRIVILEGE_ENABLED : 0;

        if (!AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
        {
            return false;
        }

        return true;
    }
}
