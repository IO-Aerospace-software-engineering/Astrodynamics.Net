// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Propagator.Forces;
using Vector3 = IO.Astrodynamics.Math.Vector3;

namespace IO.Astrodynamics.Propagator.Integrators;

public sealed class VVIntegrator : Integrator
{
    public TimeSpan DeltaT { get; }
    public double DeltaTs { get; }
    public double HalfDeltaTs { get; }
    private Vector3 _acceleration;
    private Vector3 _position;
    private Vector3 _velocity;

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

    public override void Integrate(StateVector[] result, int idx)
    {
        //Set initial parameters
        var previousElement = result[idx - 1];
        _position = previousElement.Position;
        _velocity = previousElement.Velocity;
        var prevAcc = _acceleration;

        result[idx].Position = _position + _velocity + _acceleration * 0.5 * DeltaTs * DeltaTs;

        _acceleration = ComputeAcceleration(previousElement);

        result[idx].Velocity = _velocity + (prevAcc + _acceleration) * 0.5 * DeltaTs;
    }

    public struct SV
    {
        public Vector3 Position;
        public Vector3 Velocity;

        public SV(Vector3 position, Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
        }
    }
}