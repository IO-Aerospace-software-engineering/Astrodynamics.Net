using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
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
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public Window[] Windows;
}