using System;
using System.Reflection;
using System.Runtime.InteropServices;
using IO.SDK.Net.DTO;

namespace IO.SDK.Net;

/// <summary>
/// API to communicate with IO.SDK
/// </summary>
public class API
{
    private static bool _isresolverLoaded;

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string GetSpiceVersionProxy();

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void PropagateProxy([In, Out] ref Scenario scenario);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void LaunchProxy([In, Out] ref Launch launch);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern int GetValueProxy();

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void LoadGenericKernelsProxy(string directoryPath);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string TDBToStringProxy(double secondsFromJ2000);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string UTCToStringProxy(double secondsFromJ2000);

    /// <summary>
    /// Instanciate API
    /// </summary>
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

    /// <summary>
    /// Get spice toolkit version number
    /// </summary>
    /// <returns></returns>
    public string GetSpiceVersion()
    {
        return GetSpiceVersionProxy();
    }

    /// <summary>
    /// Convert seconds from J2000 to formatted string
    /// </summary>
    /// <param name="secondsFromJ2000"></param>
    /// <returns></returns>
    public string TDBToString(double secondsFromJ2000)
    {
        return TDBToStringProxy(secondsFromJ2000);
    }

    /// <summary>
    /// Convert seconds from J2000 to formatted string
    /// </summary>
    /// <param name="secondsFromJ2000"></param>
    /// <returns></returns>
    public string UTCToString(double secondsFromJ2000)
    {
        return  UTCToStringProxy(secondsFromJ2000);
    }

    /// <summary>
    /// Execute the scenario
    /// </summary>
    /// <param name="scenario"></param>
    public void ExecuteScenario(ref Scenario scenario)
    {
        PropagateProxy(ref scenario);
    }

    /// <summary>
    /// Load generic kernel at given path
    /// </summary>
    /// <param name="directoryPath">Path where generic kernels are located</param>
    public void LoadGenericKernel(string directoryPath)
    {
        LoadGenericKernelsProxy(directoryPath);
    }
    
    /// <summary>
    /// Find launch windows
    /// </summary>
    /// <param name="launchDto"></param>
    public void FindLaunchWindows(ref Launch launchDto)
    {
        LaunchProxy(ref launchDto);
    }
}