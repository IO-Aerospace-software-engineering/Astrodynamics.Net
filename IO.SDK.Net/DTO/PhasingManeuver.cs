using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct PhasingManeuver
{
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public string[] Engines;

    public double AttitudeHoldDuration;
    public double MinimumEpoch;

    public int NumberRevolutions;
    public StateVector TargetOrbit;

    public Window ManeuverWindow;
    public Window ThrustWindow;
    public Window AttitudeWindow;
    public Vector3D DeltaV;
    public double FuelBurned;

    public PhasingManeuver() : this(-1, 0.0, double.NaN, 0, default)
    {
    }

    public PhasingManeuver(int maneuverOrder, double attitudeHoldDuration, double minimumEpoch, int numberRevolutions,
        StateVector targetOrbit)
    {
        Engines = new string[Spacecraft.ENGINESIZE];
        ManeuverOrder = maneuverOrder;
        AttitudeHoldDuration = attitudeHoldDuration;
        MinimumEpoch = minimumEpoch;
        NumberRevolutions = numberRevolutions;
        TargetOrbit = targetOrbit;
        ManeuverWindow = default;
        ThrustWindow = default;
        AttitudeWindow = default;
        DeltaV = default;
        FuelBurned = default;
    }
}