using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

public struct CombinedManeuver
{
    public int ManeuverOrder;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public int[] Engines;
    public double AttitudeHoldDuration;
    public double MinimumEpoch;

    public double TargetHeight;
    public double TargetInclination;

    public Window ManeuverWindow;
    public Window ThrustWindow;
    public Window AttitudeWindow;
    public Vector3D DeltaV;
}