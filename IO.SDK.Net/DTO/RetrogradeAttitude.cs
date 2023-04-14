using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct RetrogradeAttitude
{
    public int ManeuverOrder=-1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public IntPtr[] Engines = new IntPtr[Spacecraft.ENGINESIZE];

    double AttitudeHoldDuration = 0;
    double MinimumEpoch = 0;
    Window Window = default;

    public RetrogradeAttitude()
    {
    }
}