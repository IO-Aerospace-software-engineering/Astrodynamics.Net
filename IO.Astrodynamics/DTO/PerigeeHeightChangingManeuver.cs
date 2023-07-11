// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct PerigeeHeightChangingManeuver
{
    public readonly int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public readonly string[] Engines;

    public readonly double AttitudeHoldDuration;
    public readonly double MinimumEpoch;

    public readonly double TargetHeight;

    public Window ManeuverWindow;
    public Window ThrustWindow;
    public Window AttitudeWindow;
    public Vector3D DeltaV;
    public readonly double FuelBurned;

    public PerigeeHeightChangingManeuver() : this(-1, 0.0, double.MinValue, double.NaN)
    {
    }

    public PerigeeHeightChangingManeuver(int maneuverOrder, double attitudeHoldDuration, double minimumEpoch,
        double targetHeight)
    {
        Engines = new string[Spacecraft.ENGINESIZE];
        ManeuverOrder = maneuverOrder;
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