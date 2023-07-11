// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ApsidalAlignmentManeuver
{
    public readonly int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public readonly string[] Engines;

    public double AttitudeHoldDuration { get; }
    public double MinimumEpoch { get; }

    public StateVector TargetOrbit { get; }

    public Window ManeuverWindow { get; }
    public Window ThrustWindow { get; }
    public Window AttitudeWindow { get; }
    public Vector3D DeltaV { get; }
    public double FuelBurned { get; }
    private double Theta { get; }

    public ApsidalAlignmentManeuver() : this(-1, 0.0, double.MinValue, default)
    {
    }

    public ApsidalAlignmentManeuver(int maneuverOrder, double attitudeHoldDuration, double minimumEpoch,
        StateVector targetOrbit)
    {
        Engines = new string[Spacecraft.ENGINESIZE];
        ManeuverOrder = maneuverOrder;
        AttitudeHoldDuration = attitudeHoldDuration;
        MinimumEpoch = minimumEpoch;
        TargetOrbit = targetOrbit;
        ManeuverWindow = default;
        ThrustWindow = default;
        AttitudeWindow = default;
        DeltaV = default;
        FuelBurned = default;
        Theta = default;
    }
}