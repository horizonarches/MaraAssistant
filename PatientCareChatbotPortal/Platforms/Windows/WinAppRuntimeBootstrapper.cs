using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PatientCareChatbotPortal.Platforms.Windows;

/// <summary>
/// Initializes the Windows App Runtime bootstrapper before any WinRT types are used.
/// [ModuleInitializer] runs at the very start of the process, before Main().
/// We call the native MddBootstrapInitialize2 API directly to avoid the
/// chicken-and-egg problem where the managed auto-initializer tries to activate
/// WinRT types before the bootstrapper has set up the activation redirects.
/// </summary>
internal static class WinAppRuntimeBootstrapper
{
    // WAS 1.7.x  –  major=1, minor=7  →  packed as (major << 16 | minor) = 0x00010007
    private const uint MajorMinorVersion17 = 0x00010007;

    [StructLayout(LayoutKind.Sequential)]
    private struct PackageVersion
    {
        public ushort Revision;
        public ushort Build;
        public ushort Minor;
        public ushort Major;
    }

    // Native bootstrapper API (included in the WAS NuGet output)
    [DllImport("Microsoft.WindowsAppRuntime.Bootstrap.dll",
        CallingConvention = CallingConvention.Cdecl,
        ExactSpelling = true)]
    private static extern int MddBootstrapInitialize2(
        uint majorMinorVersion,
        [MarshalAs(UnmanagedType.LPWStr)] string? versionTag,
        PackageVersion minVersion,
        uint options);

    [ModuleInitializer]
    internal static void Initialize()
    {
        try
        {
            var minVersion = new PackageVersion(); // 0.0.0.0 → accept any 1.7.x build
            // options = 0x01 means "use existing package if already deployed"
            int hr = MddBootstrapInitialize2(MajorMinorVersion17, null, minVersion, 0x01);
            if (hr < 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[WinAppRuntimeBootstrapper] MddBootstrapInitialize2 returned 0x{hr:X8}. " +
                    "The app will attempt to continue using the installed WAS packages.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[WinAppRuntimeBootstrapper] Bootstrap exception: {ex.Message}");
        }
    }
}
