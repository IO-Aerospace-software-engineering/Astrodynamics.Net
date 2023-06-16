// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using IO.Astrodynamics.Body.Spacecraft;

namespace IO.Astrodynamics.Maneuver;

public class ProgradeAttitude : Maneuver
{
    public ProgradeAttitude(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
    {
    }
}