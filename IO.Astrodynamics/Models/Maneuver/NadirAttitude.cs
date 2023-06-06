// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Runtime.InteropServices;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Mission;

namespace IO.Astrodynamics.Models.Maneuver;

public class NadirAttitude : Maneuver
{
    public NadirAttitude(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
    {
    }
}