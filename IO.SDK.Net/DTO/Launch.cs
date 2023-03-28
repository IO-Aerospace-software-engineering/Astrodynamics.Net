using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

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

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    Window[] Windows;
}