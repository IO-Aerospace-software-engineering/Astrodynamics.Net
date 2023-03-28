using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

public struct InstrumentPointingToAttitude
{
    public int InstrumentId;
    public int TargetBodyId;
    public int TargetSiteId;
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public int[] Engines;

    public double AttitudeHoldDuration;
    public double MinimumEpoch;
    public Window Window;
}