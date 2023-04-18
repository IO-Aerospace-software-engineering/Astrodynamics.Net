using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct CombinedManeuver
{
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public string[] Engines;

    public double AttitudeHoldDuration;
    public double MinimumEpoch;

    public double TargetHeight;
    public double TargetInclination;

    public Window ManeuverWindow;
    public Window ThrustWindow;
    public Window AttitudeWindow;
    public Vector3D DeltaV;
    public double FuelBurned;

    public CombinedManeuver() : this(-1, 0.0, int.MinValue, double.NaN, double.NaN)
    {
    }

    public CombinedManeuver(int maneuverOrder, double attitudeHoldDuration, double minimumEpoch, double targetHeight,
        double targetInclination)
    {
        Engines = new string[Spacecraft.ENGINESIZE];
        ManeuverOrder = maneuverOrder;
        AttitudeHoldDuration = attitudeHoldDuration;
        MinimumEpoch = minimumEpoch;
        TargetHeight = targetHeight;
        TargetInclination = targetInclination;
        ManeuverWindow = default;
        ThrustWindow = default;
        AttitudeWindow = default;
        DeltaV = default;
        FuelBurned = default;
    }
}