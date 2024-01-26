// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Collections.Generic;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Propagator.Forces;

namespace IO.Astrodynamics.Propagator.Integrators;

public class VVIntegrator : Integrator
{
    public TimeSpan DeltaT { get; }
    public double DeltaTs { get; }
    public TimeSpan HalfDeltaT { get; }
    public double HalfDeltaTs { get; }
    private Vector3 _acceleration;

    public VVIntegrator(IEnumerable<ForceBase> forces, TimeSpan deltaT, StateVector initialState) : base(forces)
    {
        DeltaT = deltaT;
        DeltaTs = DeltaT.TotalSeconds;
        HalfDeltaTs = DeltaTs * 0.5;
        if (_acceleration == Vector3.Zero)
        {
            _acceleration = ComputeAcceleration(initialState);
        }
    }

    public override StateVector Integrate(StateVector stateVector)
    {
        //Set initial parameters
        var position = stateVector.Position;
        var velocity = stateVector.Velocity;
        var nextEpoch = stateVector.Epoch + DeltaT;

        velocity = velocity + _acceleration * HalfDeltaTs;

        position = position + velocity * DeltaTs;

        _acceleration = ComputeAcceleration(stateVector);

        velocity = velocity + _acceleration * HalfDeltaTs;

        //Create new state vector
        return new StateVector(position, velocity, stateVector.Observer, nextEpoch, stateVector.Frame);
    }
}