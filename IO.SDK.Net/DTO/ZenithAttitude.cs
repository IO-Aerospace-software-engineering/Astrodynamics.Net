using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ZenithAttitude
{
    int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public IntPtr[] Engines;

    double AttitudeHoldDuration;
    double MinimumEpoch;
    Window Window;
}