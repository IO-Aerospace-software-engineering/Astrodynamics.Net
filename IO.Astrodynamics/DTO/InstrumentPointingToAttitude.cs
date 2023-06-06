// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct InstrumentPointingToAttitude
{
    public int InstrumentId;
    public int TargetId;
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public string[] Engines;

    public double AttitudeHoldDuration;
    public double MinimumEpoch;
    public Window Window;

    public InstrumentPointingToAttitude() : this(-1, 0, 0, 0, double.MinValue)
    {
    }

    public InstrumentPointingToAttitude(int maneuverOrder, int instrumentId, int targetId,
        double attitudeHoldDuration, double minimumEpoch)
    {
        Engines = new string[Spacecraft.ENGINESIZE];
        InstrumentId = instrumentId;
        TargetId = targetId;
        ManeuverOrder = maneuverOrder;
        AttitudeHoldDuration = attitudeHoldDuration;
        MinimumEpoch = minimumEpoch;
        Window = default;
    }
}