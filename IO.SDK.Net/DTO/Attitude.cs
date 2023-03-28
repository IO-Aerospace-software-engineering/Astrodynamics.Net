using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

public struct Attitude
{
    string Name;
    int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public int[] Engines;

    double AttitudeHoldDuration;
    double MinimumEpoch;
    Window Window;
}