// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver;

public class InstrumentPointingToAttitude : Maneuver
{
    public Instrument Instrument { get; }
    public INaifObject Target { get; }

    public InstrumentPointingToAttitude(DateTime minimumEpoch, TimeSpan maneuverHoldDuration,
        Instrument instrument, INaifObject target, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, engines)
    {
        Instrument = instrument ?? throw new ArgumentNullException(nameof(instrument));
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }
}