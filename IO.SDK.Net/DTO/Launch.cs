using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Launch
{
    public int LaunchSiteId;
    public int RecoverySiteId;
    public bool LaunchByDay;
    public double InitialStepSize;
    public StateVector TargetOrbit;
    public double InertialAzimuth;
    public double NonInertialAzimuth;

    public double NonInertialInsertionVelocity;
    public double InertialInsertionVelocity;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    Window[] Windows;
}