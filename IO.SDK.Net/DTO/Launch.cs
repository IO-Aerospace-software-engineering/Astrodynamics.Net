using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Launch
{
    public Window Window;
    public Site LaunchSite;
    public Site RecoverySite;
    public bool LaunchByDay;
    public double InitialStepSize;
    public StateVector TargetOrbit;
    public double InertialAzimuth;
    public double NonInertialAzimuth;

    public double NonInertialInsertionVelocity;
    public double InertialInsertionVelocity;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    Window[] Windows;

    public Launch(Site launchSite, Site recoverySite, bool launchByDay, double initialStepSize, StateVector targetOrbit,
        Window window) : this()
    {
        Window = window;
        LaunchSite = launchSite;
        RecoverySite = recoverySite;
        LaunchByDay = launchByDay;
        InitialStepSize = initialStepSize;
        TargetOrbit = targetOrbit;
    }
}