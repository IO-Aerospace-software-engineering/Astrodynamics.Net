using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct RetrogradeAttitude
{
    int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public string[] Engines;

    double AttitudeHoldDuration;
    double MinimumEpoch;
    Window Window;
}