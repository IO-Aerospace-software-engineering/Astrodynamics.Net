// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Mission;

namespace IO.Astrodynamics.Models.Maneuver;

public class RetrogradeAttitude : Maneuver
{
    public RetrogradeAttitude(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines) : base(spacecraft,
        minimumEpoch, maneuverHoldDuration, engines)
    {
    }
}