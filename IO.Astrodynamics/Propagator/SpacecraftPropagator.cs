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
using Quaternion = IO.Astrodynamics.Math.Quaternion;
using Vector3 = IO.Astrodynamics.Math.Vector3;

namespace IO.Astrodynamics.Propagator;

public class SpacecraftPropagator
{
    public Window Window { get; }
    public IEnumerable<CelestialBody> CelestialBodies { get; }
    public bool IncludeAtmosphericDrag { get; }
    public bool IncludeSolarRadiationPressure { get; }
    public Spacecraft Spacecraft { get; }
    public Integrator Integrator { get; }
    public TimeSpan DeltaT { get; }

    private uint _svCacheSize;
    private StateVector[] _svCache;
    private Dictionary<DateTime, StateOrientation> _stateOrientation = new Dictionary<DateTime, StateOrientation>();


    /// <summary>
    /// Instantiate propagator
    /// </summary>
    /// <param name="window">Time window</param>
    /// <param name="spacecraft"></param>
    /// <param name="additionalCelestialBodies">Additional celestial bodies</param>
    /// <param name="includeAtmosphericDrag"></param>
    /// <param name="includeSolarRadiationPressure"></param>
    /// <param name="deltaT">Simulation step size</param>
    /// <exception cref="ArgumentNullException"></exception>
    public SpacecraftPropagator(Window window, Spacecraft spacecraft, IEnumerable<CelestialBody> additionalCelestialBodies, bool includeAtmosphericDrag,
        bool includeSolarRadiationPressure, TimeSpan deltaT)
    {
        Window = window;
        CelestialBodies = new[] { spacecraft.InitialOrbitalParameters.Observer as CelestialBody }.Concat(additionalCelestialBodies??Array.Empty<CelestialBody>());
        IncludeAtmosphericDrag = includeAtmosphericDrag;
        IncludeSolarRadiationPressure = includeSolarRadiationPressure;
        DeltaT = deltaT;
        Spacecraft = spacecraft ?? throw new ArgumentNullException(nameof(spacecraft));

        var forces = InitializeForces(includeAtmosphericDrag, includeSolarRadiationPressure);

        Integrator = new VVIntegrator(forces, DeltaT, Spacecraft.InitialOrbitalParameters.AtEpoch(Window.StartDate).ToStateVector());
        _svCacheSize = (uint)Window.Length.TotalSeconds / (uint)DeltaT.TotalSeconds;
        _svCache = new StateVector[_svCacheSize];
        StateVector stateVector = Spacecraft.InitialOrbitalParameters.AtEpoch(Window.StartDate).ToStateVector();
        _svCache[0] = stateVector;
        for (int i = 1; i < _svCacheSize; i++)
        {
            _svCache[i] = new StateVector(Vector3.Zero, Vector3.Zero, stateVector.Observer, Window.StartDate + (i * DeltaT), stateVector.Frame);
        }
    }

    private List<ForceBase> InitializeForces(bool includeAtmosphericDrag, bool includeSolarRadiationPressure)
    {
        List<ForceBase> forces = new List<ForceBase>();
        foreach (var celestialBody in CelestialBodies.Distinct())
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

    //Todo optimize by unrolling loop
    public (IEnumerable<StateVector>stateVectors, IEnumerable<StateOrientation>stateOrientations) Propagate()
    {
        _stateOrientation[Window.StartDate] = new StateOrientation(Quaternion.Zero, Vector3.Zero, Window.StartDate, Spacecraft.InitialOrbitalParameters.Frame);
        for (int i = 1; i < _svCacheSize; i++)
        {
            var prvSv = _svCache[i - 1];
            if (Spacecraft.StandbyManeuver?.CanExecute(prvSv) == true)
            {
                var res = Spacecraft.StandbyManeuver.TryExecute(prvSv);
                _stateOrientation[res.so.Epoch] = res.so;
            }

            Integrator.Integrate(_svCache, i);
        }

        _stateOrientation[Window.EndDate] = new StateOrientation(_stateOrientation.Last().Value.Rotation, Vector3.Zero, Window.EndDate, Spacecraft.InitialOrbitalParameters.Frame);

        return (_svCache, _stateOrientation.Values);
    }
}