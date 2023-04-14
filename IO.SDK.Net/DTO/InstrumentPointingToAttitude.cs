using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct InstrumentPointingToAttitude
{
    public int InstrumentId = 0;
    public int TargetBodyId = -1;
    public int TargetSiteId = -1;
    public int ManeuverOrder=-1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public IntPtr[] Engines = new IntPtr[Spacecraft.ENGINESIZE];

    public double AttitudeHoldDuration = 0;
    public double MinimumEpoch = 0;
    public Window Window = default;

    public InstrumentPointingToAttitude()
    {
    }
}