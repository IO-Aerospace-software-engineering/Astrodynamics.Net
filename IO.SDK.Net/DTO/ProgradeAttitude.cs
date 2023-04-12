using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ProgradeAttitude
{
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public string[] Engines;

    public double AttitudeHoldDuration;
    public double MinimumEpoch;
    public Window Window;
}