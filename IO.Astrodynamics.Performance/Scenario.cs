// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using BenchmarkDotNet.Attributes;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Physics;
using IO.Astrodynamics.Propagator.Forces;
using IO.Astrodynamics.Propagator.Integrators;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Performance;

[MarkdownExporterAttribute.GitHub]
[MemoryDiagnoser]
[SkewnessColumn]
[KurtosisColumn]
[StatisticalTestColumn]
[ShortRunJob]
public class Scenario
{
    private readonly GeopotentialGravitationalField _geopotential;
    private readonly SolarRadiationPressure _srp;
    private readonly AtmosphericDrag _atm;
    private readonly CelestialBody _earth;
    private readonly CelestialBody _sun;
    private readonly CelestialBody _moon;
    private readonly VVIntegrator _integrator;
    private readonly Propagator.Propagator _propagator;
    // IO.Astrodynamics.Tests.Mission.ScenarioTests _scenario = new IO.Astrodynamics.Tests.Mission.ScenarioTests();

    public Scenario()
    {
        API.Instance.LoadKernels(new DirectoryInfo("Data"));
        _earth = new CelestialBody(399, new GeopotentialModelParameters("Data/SolarSystem/EGM2008_to70_TideFree", 30), new EarthAtmosphericModel());
        _moon = new CelestialBody(301);
        _sun = new CelestialBody(10);
        _geopotential = new GeopotentialGravitationalField(new StreamReader("Data/SolarSystem/EGM2008_to70_TideFree"));
        Clock clk = new Clock("My clock", 1.0 / 256.0);
        Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 100.0, 10000.0, clk,
            new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), _earth, DateTimeExtension.J2000, Frames.Frame.ICRF));
        _srp = new SolarRadiationPressure(spc);
        _atm = new AtmosphericDrag(spc);
        List<ForceBase> forces = new List<ForceBase>();
        forces.Add(new GravitationalAcceleration(_sun));
        forces.Add(new GravitationalAcceleration(_moon));
        forces.Add(new GravitationalAcceleration(_earth));
        forces.Add(new AtmosphericDrag(spc));
        forces.Add(new SolarRadiationPressure(spc));
        _integrator = new VVIntegrator(forces, TimeSpan.FromSeconds(1.0), new StateVector(new Vector3(6800000.0 - Random.Shared.NextDouble(), 0.0, 0.0),
            new Vector3(0.0, 8000.0 - Random.Shared.NextDouble(), 0.0), _earth,
            DateTimeExtension.J2000, Frame.ICRF));
        _propagator = new Propagator.Propagator(new Window(DateTimeExtension.J2000, DateTimeExtension.J2000 + spc.InitialOrbitalParameters.Period()), spc,
            new[] { _moon, _earth, _sun }, true, true, TimeSpan.FromSeconds(1.0));
    }

    // [Benchmark(Description = "Spacecraft propagator C++")]
    public void Propagate()
    {
        // _scenario.PropagateWithoutManeuver();
    }

    // [Benchmark(Description = "Compute gravitational acceleration")]
    public void Gravity()
    {
        var sv = new StateVector(new Vector3(6800000.0 - Random.Shared.NextDouble(), 0.0, 0.0), new Vector3(0.0, 8000.0 - Random.Shared.NextDouble(), 0.0), _earth,
            DateTimeExtension.J2000, Frame.ICRF);
        var res = _geopotential.ComputeGravitationalAcceleration(sv);
    }

    // [Benchmark(Description = "Solar radiation pressure")]
    public void SRP()
    {
        var sv = new StateVector(new Vector3(6800000.0 - Random.Shared.NextDouble(), 0.0, 0.0), new Vector3(0.0, 8000.0 - Random.Shared.NextDouble(), 0.0), _earth,
            DateTimeExtension.J2000, Frame.ICRF);
        var res = _srp.Apply(sv);
    }

    // [Benchmark(Description = "Atmospheric drag")]
    public void AtmosphericDrag()
    {
        var sv = new StateVector(new Vector3(6800000.0 - Random.Shared.NextDouble(), 0.0, 0.0), new Vector3(0.0, 8000.0 - Random.Shared.NextDouble(), 0.0), _earth,
            DateTimeExtension.J2000, Frame.ICRF);
        var res = _atm.Apply(sv);
    }


    // [Benchmark(Description = "VV integrator")]
    public void VVIntegration()
    {
        var sv = new StateVector(new Vector3(6800000.0 - Random.Shared.NextDouble(), 0.0, 0.0), new Vector3(0.0, 8000.0 - Random.Shared.NextDouble(), 0.0), _earth,
            DateTimeExtension.J2000, Frame.ICRF);
        // var res = _integrator.Integrate(sv);
    }

    [Benchmark(Description = "Propagator per orbit (GeoPotentials // Moon and sun perturbation // Atmospheric drag // Solar radiation) ")]
    public void Propagator()
    {
        var res = _propagator.Propagate();
    }
}