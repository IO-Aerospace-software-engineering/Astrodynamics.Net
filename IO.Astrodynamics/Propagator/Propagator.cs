// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Propagator.Forces;
using IO.Astrodynamics.Propagator.Integrators;
using IO.Astrodynamics.Time;
using Vector3 = IO.Astrodynamics.Math.Vector3;

namespace IO.Astrodynamics.Propagator;

public class Propagator
{
    public Window Window { get; }
    public IEnumerable<CelestialBody> CelestialBodies { get; }
    public bool IncludeAtmosphericDrag { get; }
    public bool IncludeSolarRadiationPressure { get; }
    public Spacecraft Spacecraft { get; }
    public Integrator Integrator { get; }
    public TimeSpan DeltaT { get; }

    private uint _dataCacheSize;
    private StateVector[] _dataCache;


    public Propagator(Window window, Spacecraft spacecraft, IEnumerable<CelestialBody> celestialBodies, bool includeAtmosphericDrag,
        bool includeSolarRadiationPressure, TimeSpan deltaT)
    {
        Window = window;
        CelestialBodies = celestialBodies ?? throw new ArgumentNullException(nameof(celestialBodies));
        IncludeAtmosphericDrag = includeAtmosphericDrag;
        IncludeSolarRadiationPressure = includeSolarRadiationPressure;
        DeltaT = deltaT;
        Spacecraft = spacecraft ?? throw new ArgumentNullException(nameof(spacecraft));

        var forces = InitializeForces(includeAtmosphericDrag, includeSolarRadiationPressure);

        Integrator = new VVIntegrator(forces, DeltaT, Spacecraft.InitialOrbitalParameters.AtEpoch(Window.StartDate).ToStateVector());
        StateVector stateVector = Spacecraft.InitialOrbitalParameters.AtEpoch(Window.StartDate).ToStateVector();
        // result.Add(stateVector);
        _dataCacheSize = (uint)Window.Length.TotalSeconds / (uint)DeltaT.TotalSeconds;
        _dataCache = new StateVector[_dataCacheSize];
        Array.Fill(_dataCache, new StateVector(Vector3.Zero, Vector3.Zero, stateVector.Observer, stateVector.Epoch, stateVector.Frame), 0, (int)_dataCacheSize);
        _dataCache[0] = stateVector;
    }

    private List<ForceBase> InitializeForces(bool includeAtmosphericDrag, bool includeSolarRadiationPressure)
    {
        List<ForceBase> forces = new List<ForceBase>();
        foreach (var celestialBody in CelestialBodies)
        {
            forces.Add(new GravitationalAcceleration(celestialBody));
        }

        if (includeAtmosphericDrag)
        {
            forces.Add(new AtmosphericDrag(Spacecraft));
        }

        if (includeSolarRadiationPressure)
        {
            forces.Add(new SolarRadiationPressure(Spacecraft));
        }

        return forces;
    }

    public IEnumerable<StateVector> Propagate()
    {
        for (int i = 1; i < _dataCacheSize; i++)
        {
            Integrator.Integrate(_dataCache, i);
        }

        // return results;
        return _dataCache;
    }
}