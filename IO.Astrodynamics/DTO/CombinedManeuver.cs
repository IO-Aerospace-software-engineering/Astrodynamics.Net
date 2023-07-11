// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct CombinedManeuver
{
    public readonly int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public readonly string[] Engines;

    public readonly double AttitudeHoldDuration;
    public readonly double MinimumEpoch;

    public readonly double TargetHeight;
    public readonly double TargetInclination;

    public Window ManeuverWindow;
    public Window ThrustWindow;
    public Window AttitudeWindow;
    public Vector3D DeltaV;
    public readonly double FuelBurned;

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