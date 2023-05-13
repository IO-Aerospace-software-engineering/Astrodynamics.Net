using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct PerigeeHeightChangingManeuver
{
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public string[] Engines ;

    public double AttitudeHoldDuration;
    public double MinimumEpoch ;

    public double TargetHeight ;

    public Window ManeuverWindow ;
    public Window ThrustWindow ;
    public Window AttitudeWindow ;
    public Vector3D DeltaV ;
    public double FuelBurned ;

    public PerigeeHeightChangingManeuver():this(-1,0.0,double.MinValue,double.NaN)
    {
    }

    public PerigeeHeightChangingManeuver(int maneuverOrder, double attitudeHoldDuration, double minimumEpoch, double targetHeight)
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