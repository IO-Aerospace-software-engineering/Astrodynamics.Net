// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ProgradeAttitude
{
    public readonly int ManeuverOrder = -1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public readonly string[] Engines;

    public readonly double AttitudeHoldDuration;
    public readonly double MinimumEpoch;
    public Window Window;

    public ProgradeAttitude() : this(-1, 0.0, double.MinValue)
    {
    }

    public ProgradeAttitude(int maneuverOrder, double attitudeHoldDuration, double minimumEpoch)
    {
        Engines = new string[Spacecraft.ENGINESIZE];
        ManeuverOrder = maneuverOrder;
        AttitudeHoldDuration = attitudeHoldDuration;
        MinimumEpoch = minimumEpoch;
        Window = default;
    }
}