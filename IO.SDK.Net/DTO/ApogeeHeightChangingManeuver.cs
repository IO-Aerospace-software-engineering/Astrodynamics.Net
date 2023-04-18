using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ApogeeHeightChangingManeuver
{
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public string[] Engines;

    public double AttitudeHoldDuration;
    public double MinimumEpoch;

    public double TargetHeight;

    public Window ManeuverWindow;
    public Window ThrustWindow;
    public Window AttitudeWindow;
    public Vector3D DeltaV;
    public double FuelBurned;

    public ApogeeHeightChangingManeuver() : this(-1, 0.0, double.MinValue, double.NaN)
    {
    }

    public ApogeeHeightChangingManeuver(int maneuverOrder, double attitudeHoldDuration, double minimumEpoch,
        double targetHeight)
    {
        ManeuverOrder = maneuverOrder;
        Engines = new string[Spacecraft.ENGINESIZE];
        AttitudeHoldDuration = attitudeHoldDuration;
        MinimumEpoch = minimumEpoch;
        TargetHeight = targetHeight;
        ManeuverWindow = default;
        ThrustWindow = default;
        AttitudeWindow = default;
        DeltaV = default;
        FuelBurned = default;
    }
}