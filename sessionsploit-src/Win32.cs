using System;
using System.Runtime.InteropServices;

internal class Win32
{
  public const int SE_PRIVILEGE_ENABLED = 2;
  public const int TOKEN_QUERY = 8;
  public const int TOKEN_ADJUST_PRIVILEGES = 32;
  public const uint STANDARD_RIGHTS_REQUIRED = 983040;
  public const uint STANDARD_RIGHTS_READ = 131072;
  public const uint TOKEN_ASSIGN_PRIMARY = 1;
  public const uint TOKEN_DUPLICATE = 2;
  public const uint TOKEN_IMPERSONATE = 4;
  public const uint TOKEN_QUERY_SOURCE = 16;
  public const uint TOKEN_ADJUST_GROUPS = 64;
  public const uint TOKEN_ADJUST_DEFAULT = 128;
  public const uint TOKEN_ADJUST_SESSIONID = 256;
  public const uint TOKEN_READ = 131080;
  public const uint TOKEN_ALL_ACCESS = 983551;
  public const string SE_TIME_ZONE_NAMETEXT = "SeTimeZonePrivilege";
  public const int ANYSIZE_ARRAY = 1;

  [DllImport("wtsapi32.dll", SetLastError = true)]
  public static extern bool WTSEnumerateSessions(
    IntPtr hServer,
    int Reserved,
    int Version,
    out IntPtr ppSessionInfo,
    out int pCount);

  [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  public static extern int WTSConnectSession(
    int targetSessionId,
    int sourceSessionId,
    string password,
    bool wait);

  [DllImport("wtsapi32.dll", SetLastError = true)]
  public static extern int WTSDisconnectSession(IntPtr hServer, int sessionId, bool bWait);

  [DllImport("kernel32.dll")]
  public static extern int WTSGetActiveConsoleSessionId();

  [DllImport("wtsapi32.dll", SetLastError = true)]
  public static extern void WTSFreeMemory(IntPtr memory);

  [DllImport("wtsapi32.dll")]
  public static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] string pServerName);

  [DllImport("wtsapi32.dll")]
  public static extern void WTSCloseServer(IntPtr hServer);

  [DllImport("wtsapi32.dll")]
  public static extern bool WTSQuerySessionInformation(
    IntPtr hServer,
    int sessionId,
    Win32.WTS_INFO_CLASS wtsInfoClass,
    out IntPtr ppBuffer,
    out uint pBytesReturned);

  [DllImport("advapi32.dll", SetLastError = true)]
  public static extern bool DuplicateToken(
    IntPtr ExistingTokenHandle,
    int SECURITY_IMPERSONATION_LEVEL,
    out IntPtr DuplicateTokenHandle);

  [DllImport("advapi32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static extern bool SetThreadToken(IntPtr PHThread, IntPtr Token);

  [DllImport("advapi32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static extern bool OpenProcessToken(
    IntPtr ProcessHandle,
    uint DesiredAccess,
    out IntPtr TokenHandle);

  [DllImport("advapi32.dll", SetLastError = true)]
  public static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

  [DllImport("kernel32.dll")]
  public static extern IntPtr GetCurrentProcess();

  [DllImport("advapi32.dll", SetLastError = true)]
  public static extern bool AdjustTokenPrivileges(
    IntPtr htok,
    bool disall,
    ref Win32.TokPriv1Luid newst,
    int len,
    IntPtr prev,
    IntPtr relen);

  public enum WTS_CONNECTSTATE_CLASS
  {
    WTSActive,
    WTSConnected,
    WTSConnectQuery,
    WTSShadow,
    WTSDisconnected,
    WTSIdle,
    WTSListen,
    WTSReset,
    WTSDown,
    WTSInit,
  }

  public enum WTS_INFO_CLASS
  {
    WTSInitialProgram,
    WTSApplicationName,
    WTSWorkingDirectory,
    WTSOEMId,
    WTSSessionId,
    WTSUserName,
    WTSWinStationName,
    WTSDomainName,
    WTSConnectState,
    WTSClientBuildNumber,
    WTSClientName,
    WTSClientDirectory,
    WTSClientProductId,
    WTSClientHardwareId,
    WTSClientAddress,
    WTSClientDisplay,
    WTSClientProtocolType,
  }

  public struct WTS_SESSION_INFO
  {
    public int SessionId;
    public IntPtr pWinStationName;
    public Win32.WTS_CONNECTSTATE_CLASS State;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TokPriv1Luid
  {
    public int Count;
    public long Luid;
    public int Attr;
  }

  public struct LUID
  {
    public uint LowPart;
    public uint HighPart;
  }

  public struct LUID_AND_ATTRIBUTES
  {
    public Win32.LUID Luid;
    public uint Attributes;
  }

  public struct TOKEN_PRIVILEGES
  {
    public uint PrivilegeCount;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
    public Win32.LUID_AND_ATTRIBUTES[] Privileges;
  }
}
