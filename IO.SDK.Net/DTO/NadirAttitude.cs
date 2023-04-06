using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct NadirAttitude
{
    int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string Engines;

    double AttitudeHoldDuration;
    double MinimumEpoch;
    Window Window;
}