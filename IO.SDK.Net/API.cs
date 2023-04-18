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
    public static extern void PropagateProxy([In, Out] ref Scenario scenario);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void LaunchProxy([In, Out] ref Launch launch);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int GetValueProxy();

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void LoadGenericKernelsProxy(string directoryPath);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr TDBToStringProxy(double secondsFromJ2000);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr UTCToStringProxy(double secondsFromJ2000);

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
                sharedLibName = "resources/IO.SDK.dll";
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

    public string TDBToString(double secondsFromJ2000)
    {
        IntPtr res = TDBToStringProxy(secondsFromJ2000);
        var str = Marshal.PtrToStringAnsi(res);
        return str;
    }

    public string UTCToString(double secondsFromJ2000)
    {
        IntPtr res = UTCToStringProxy(secondsFromJ2000);
        var str = Marshal.PtrToStringAnsi(res);
        return str;
    }

    public void ExecuteScenario(ref Scenario scenario)
    {
        PropagateProxy(ref scenario);
    }

    public int GetValue()
    {
        return GetValueProxy();
    }

    public void LoadGenericKernel(string directoryPath)
    {
        LoadGenericKernelsProxy(directoryPath);
    }
}