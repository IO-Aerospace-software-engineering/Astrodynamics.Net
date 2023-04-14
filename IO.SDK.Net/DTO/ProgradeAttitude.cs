using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ProgradeAttitude
{
    public int ManeuverOrder=-1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public string[] Engines = new string[Spacecraft.ENGINESIZE];

    public double AttitudeHoldDuration = 0;
    public double MinimumEpoch = 0;
    public Window Window = default;

    public ProgradeAttitude()
    {
    }
}