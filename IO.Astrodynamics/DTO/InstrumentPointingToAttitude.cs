// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct InstrumentPointingToAttitude
{
    public int InstrumentId;
    public int TargetBodyId;
    public int TargetSiteId;
    public int ManeuverOrder;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Spacecraft.ENGINESIZE)]
    public string[] Engines;

    public double AttitudeHoldDuration;
    public double MinimumEpoch;
    public Window Window;

    public InstrumentPointingToAttitude() : this(-1, 0, 0, 0, 0, double.MinValue)
    {
    }

    public InstrumentPointingToAttitude(int maneuverOrder, int instrumentId, int targetBodyId, int targetSiteId,
        double attitudeHoldDuration, double minimumEpoch)
    {
        Engines = new string[Spacecraft.ENGINESIZE];
        InstrumentId = instrumentId;
        TargetBodyId = targetBodyId;
        TargetSiteId = targetSiteId;
        ManeuverOrder = maneuverOrder;
        AttitudeHoldDuration = attitudeHoldDuration;
        MinimumEpoch = minimumEpoch;
        Window = default;
    }
}