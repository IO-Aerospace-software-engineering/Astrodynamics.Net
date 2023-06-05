// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ApogeeHeightChangingManeuver
{
    public int ManeuverOrder { get; }

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public readonly string[] Engines;

    public double AttitudeHoldDuration { get; }
    public double MinimumEpoch { get; }

    public double TargetHeight { get; }

    public Window ManeuverWindow { get; }
    public Window ThrustWindow { get; }
    public Window AttitudeWindow { get; }
    public Vector3D DeltaV { get; }
    public double FuelBurned { get; }

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