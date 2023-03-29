using System;
using System.Reflection;
using System.Runtime.InteropServices;
using IO.SDK.Net.DTO;

namespace IO.SDK.Net;

public class API
{
    private static bool _isresolverLoaded;

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr GetSpiceVersionProxy();

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern Scenario Execute(Scenario scenario);

    public API()
    {
        if (!_isresolverLoaded)
        {
            NativeLibrary.SetDllImportResolver(typeof(API).Assembly, Resolver);
            _isresolverLoaded = true;
        }
    }

    private static IntPtr Resolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        IntPtr libHandle = IntPtr.Zero;

        if (libraryName == "IO.SDK")
        {
            string sharedLibName = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                sharedLibName = "IO.SDK.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                sharedLibName = "resources/libIO.SDK.so";
            }

            if (!string.IsNullOrEmpty(sharedLibName))
            {
                NativeLibrary.TryLoad(sharedLibName, typeof(API).Assembly, DllImportSearchPath.AssemblyDirectory,
                    out libHandle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        return libHandle;
    }

    public string GetSpiceVersion()
    {
        IntPtr res = GetSpiceVersionProxy();
        var str = Marshal.PtrToStringAnsi(res);
        return str;
    }

    public Scenario ExecuteScenario(Scenario scenario)
    {
        return Execute(scenario);
    }
}