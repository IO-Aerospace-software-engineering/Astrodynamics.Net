using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct InstrumentPointingToAttitude
{
    public int InstrumentId;
    public int TargetBodyId;
    public int TargetSiteId;
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string Engines;

    public double AttitudeHoldDuration;
    public double MinimumEpoch;
    public Window Window;
}