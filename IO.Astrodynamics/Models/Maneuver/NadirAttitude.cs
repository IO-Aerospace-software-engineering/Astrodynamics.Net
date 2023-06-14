// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using IO.Astrodynamics.Models.Body.Spacecraft;

namespace IO.Astrodynamics.Models.Maneuver;

public class NadirAttitude : Maneuver
{
    public NadirAttitude(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
    {
    }
}