using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ApogeeHeightChangingManeuver
{
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public IntPtr[] Engines;

    public double AttitudeHoldDuration;
    public double MinimumEpoch;

    public double TargetHeight;

    public Window ManeuverWindow;
    public Window ThrustWindow;
    public Window AttitudeWindow;
    public Vector3D DeltaV;

    public ApogeeHeightChangingManeuver()
    {
        ManeuverOrder = -1;
        Engines = new IntPtr[10];
        AttitudeHoldDuration = 0;
        MinimumEpoch = 0;
        TargetHeight = 0;
        ManeuverWindow = default;
        ThrustWindow = default;
        AttitudeWindow = default;
        DeltaV = default;
    }
}