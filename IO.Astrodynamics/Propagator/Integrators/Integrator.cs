// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Collections.Generic;
using System.Linq;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Propagator.Forces;

namespace IO.Astrodynamics.Propagator.Integrators;

public abstract class Integrator
{
    public IReadOnlyCollection<ForceBase> Forces { get; }

    public abstract StateVector Integrate(StateVector stateVector);

    public Integrator(IEnumerable<ForceBase> forces)
    {
        Forces = new List<ForceBase>(forces);
    }

    public virtual Vector3 ComputeAcceleration(StateVector stateVector)
    {
        Vector3 res = Vector3.Zero;
        foreach (var force in Forces)
        {
            res += force.Apply(stateVector);
        }
        return res;
    }
}