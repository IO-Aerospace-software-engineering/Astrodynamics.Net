using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct OrbitalPlaneChangingManeuver
{
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string Engines;
    public double AttitudeHoldDuration;
    public double MinimumEpoch;

    public StateVector TargetOrbit;

    public Window ManeuverWindow;
    public Window ThrustWindow;
    public Window AttitudeWindow;
    public Vector3D DeltaV;
}