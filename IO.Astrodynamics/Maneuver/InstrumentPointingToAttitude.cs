// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver;

public class InstrumentPointingToAttitude : Attitude
{
    public Instrument Instrument { get; }
    public ILocalizable Target { get; }

    public InstrumentPointingToAttitude(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, Instrument instrument, ILocalizable target, Engine engine) : base(minimumEpoch,
        maneuverHoldDuration, engine)
    {
        Instrument = instrument ?? throw new ArgumentNullException(nameof(instrument));
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    protected override Quaternion ComputeOrientation(StateVector stateVector)
    {
        var ephemeris = Target.GetEphemeris(stateVector.Epoch, stateVector.Observer, stateVector.Frame, Aberration.LT);
        var targetVector = ephemeris.ToStateVector().Position - stateVector.Position;
        return Instrument.GetBoresightInSpacecraftFrame().To(targetVector);
    }
}