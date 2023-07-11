// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct RetrogradeAttitude
{
    public readonly int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public readonly string[] Engines;

    private readonly double AttitudeHoldDuration;
    private readonly double MinimumEpoch;
    private readonly Window Window;

    public RetrogradeAttitude() : this(-1, 0.0, double.MinValue)
    {
    }

    public RetrogradeAttitude(int maneuverOrder, double attitudeHoldDuration, double minimumEpoch)
    {
        Engines = new string[Spacecraft.ENGINESIZE];
        ManeuverOrder = maneuverOrder;
        AttitudeHoldDuration = attitudeHoldDuration;
        MinimumEpoch = minimumEpoch;
        Window = default;
    }
}