﻿using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using IO.SDK.Net.DTO;

namespace IO.SDK.Net;

/// <summary>
/// API to communicate with IO.SDK
/// </summary>
public class API
{
    private static bool _isResolverLoaded;

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string GetSpiceVersionProxy();

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void PropagateProxy([In, Out] ref Scenario scenario);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void LaunchProxy([In, Out] ref Launch launch);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void LoadKernelsProxy(string directoryPath);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string TDBToStringProxy(double secondsFromJ2000);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string UTCToStringProxy(double secondsFromJ2000);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void FindWindowsOnDistanceConstraintProxy(Window searchWindow, int observerId,
        int targetId, string constraint, double value, string aberration, double stepSize, [In, Out] Window[] windows);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void FindWindowsOnOccultationConstraintProxy(Window searchWindow, int observerId,
        int targetId,
        string targetFrame, string targetShape, int frontBodyId, string frontFrame, string frontShape,
        string occultationType,
        string aberration, double stepSize, [In, Out] Window[] windows);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void FindWindowsOnCoordinateConstraintProxy(Window searchWindow, int observerId, int targetId,
        string frame, string coordinateSystem, string coordinate,
        string relationalOperator, double value, double adjustValue, string aberration, double stepSize,
        [In, Out] Window[] windows);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void FindWindowsOnIlluminationConstraintProxy(Window searchWindow, int observerId,
        string illuminationSource, int targetBody, string fixedFrame,
        Geodetic geodetic, string illuminationType, string relationalOperator, double value, double adjustValue,
        string aberration, double stepSize, string method, [In, Out] Window[] windows);

    [DllImport(@"IO.SDK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void FindWindowsInFieldOfViewConstraintProxy(Window searchWindow, int observerId,
        int instrumentId, int targetId, string targetFrame, string targetShape, string aberration, double stepSize,
        [In, Out] Window[] windows);

    /// <summary>
    /// Instantiate API
    /// </summary>
    public API()
    {
        if (_isResolverLoaded) return;
        NativeLibrary.SetDllImportResolver(typeof(API).Assembly, Resolver);
        _isResolverLoaded = true;
    }

    private static IntPtr Resolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        IntPtr libHandle = IntPtr.Zero;

        if (libraryName != "IO.SDK") return libHandle;
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
        return UTCToStringProxy(secondsFromJ2000);
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
    /// <param name="path">Path where kernels are located. This could be a file path or a directory path</param>
    public void LoadKernels(string path)
    {
        LoadKernelsProxy(path);
    }

    /// <summary>
    /// Find launch windows
    /// </summary>
    /// <param name="launchDto"></param>
    public void FindLaunchWindows(ref Launch launchDto)
    {
        LaunchProxy(ref launchDto);
    }

    /// <summary>
    /// Find time windows based on distance constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="targetId"></param>
    /// <param name="constraint"></param>
    /// <param name="value"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public Window[] FindWindowsOnDistanceConstraint(Window searchWindow, int observerId,
        int targetId, string constraint, double value, string aberration, double stepSize)
    {
        Window[] windows = new Window[1000];
        for (int i = 0; i < 1000; i++)
        {
            windows[i].Start = double.NaN;
            windows[i].End = double.NaN;
        }
        FindWindowsOnDistanceConstraintProxy(searchWindow, observerId, targetId, constraint, value, aberration,
            stepSize, windows);
        return windows.Where(x => !double.IsNaN(x.Start)).ToArray();
    }

    /// <summary>
    /// Find time windows based on occultation constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="targetId"></param>
    /// <param name="targetFrame"></param>
    /// <param name="targetShape"></param>
    /// <param name="frontBodyId"></param>
    /// <param name="frontFrame"></param>
    /// <param name="frontShape"></param>
    /// <param name="occultationType"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public Window[] FindWindowsOnOccultationConstraint(Window searchWindow, int observerId,
        int targetId, string targetFrame, string targetShape, int frontBodyId, string frontFrame, string frontShape,
        string occultationType, string aberration, double stepSize)
    {
        Window[] windows = new Window[1000];
        for (int i = 0; i < 1000; i++)
        {
            windows[i].Start = double.NaN;
            windows[i].End = double.NaN;
        }
        FindWindowsOnOccultationConstraintProxy(searchWindow, observerId, targetId, targetFrame, targetShape,
            frontBodyId, frontFrame, frontShape, occultationType, aberration, stepSize, windows);
        return windows.Where(x => !double.IsNaN(x.Start)).ToArray();
    }

    /// <summary>
    /// Find time windows based on coordinate constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="targetId"></param>
    /// <param name="frame"></param>
    /// <param name="coordinateSystem"></param>
    /// <param name="coordinate"></param>
    /// <param name="relationalOperator"></param>
    /// <param name="value"></param>
    /// <param name="adjustValue"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public Window[] FindWindowsOnCoordinateConstraint(Window searchWindow, int observerId, int targetId,
        string frame, string coordinateSystem, string coordinate,
        string relationalOperator, double value, double adjustValue, string aberration, double stepSize)
    {
        Window[] windows = new Window[1000];
        for (int i = 0; i < 1000; i++)
        {
            windows[i].Start = double.NaN;
            windows[i].End = double.NaN;
        }
        FindWindowsOnCoordinateConstraintProxy(searchWindow, observerId, targetId, frame, coordinateSystem,
            coordinate, relationalOperator, value, adjustValue, aberration, stepSize, windows);
        return windows.Where(x => !double.IsNaN(x.Start)).ToArray();
    }

    /// <summary>
    /// Find time windows based on illumination constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="illuminationSource"></param>
    /// <param name="targetBody"></param>
    /// <param name="fixedFrame"></param>
    /// <param name="geodetic"></param>
    /// <param name="illuminationType"></param>
    /// <param name="relationalOperator"></param>
    /// <param name="value"></param>
    /// <param name="adjustValue"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public Window[] FindWindowsOnIlluminationConstraint(Window searchWindow, int observerId,
        string illuminationSource, int targetBody, string fixedFrame,
        Geodetic geodetic, string illuminationType, string relationalOperator, double value, double adjustValue,
        string aberration, double stepSize, string method)
    {
        Window[] windows = new Window[1000];
        for (int i = 0; i < 1000; i++)
        {
            windows[i].Start = double.NaN;
            windows[i].End = double.NaN;
        }
        FindWindowsOnIlluminationConstraintProxy(searchWindow, observerId, illuminationSource, targetBody, fixedFrame,
            geodetic, illuminationType, relationalOperator, value, adjustValue, aberration, stepSize, method, windows);
        return windows.Where(x => !double.IsNaN(x.Start)).ToArray();
    }

    /// <summary>
    /// Find time window when a target is in instrument's field of view
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="instrumentId"></param>
    /// <param name="targetId"></param>
    /// <param name="targetFrame"></param>
    /// <param name="targetShape"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public Window[] FindWindowsInFieldOfViewConstraint(Window searchWindow, int observerId,
        int instrumentId, int targetId, string targetFrame, string targetShape, string aberration, double stepSize)
    {
        Window[] windows = new Window[1000];
        for (int i = 0; i < 1000; i++)
        {
            windows[i].Start = double.NaN;
            windows[i].End = double.NaN;
        }

        FindWindowsInFieldOfViewConstraintProxy(searchWindow, observerId, instrumentId, targetId, targetFrame,
            targetShape, aberration, stepSize, windows);
        return windows.Where(x => !double.IsNaN(x.Start)).ToArray();
    }
}