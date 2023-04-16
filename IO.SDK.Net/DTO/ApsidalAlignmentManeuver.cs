using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ApsidalAlignmentManeuver
{
    public int ManeuverOrder=-1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public IntPtr[] Engines = new IntPtr[Spacecraft.ENGINESIZE];

    public double AttitudeHoldDuration = 0;
    public double MinimumEpoch = 0;

    public StateVector TargetOrbit = default;

    public Window ManeuverWindow = default;
    public Window ThrustWindow = default;
    public Window AttitudeWindow = default;
    public Vector3D DeltaV = default;
    public double FuelBurned = 0;
    double Theta = 0;

    public ApsidalAlignmentManeuver()
    {
    }
}