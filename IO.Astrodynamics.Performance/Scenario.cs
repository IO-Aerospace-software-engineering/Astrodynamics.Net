// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using BenchmarkDotNet.Attributes;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
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
    private readonly CelestialBody _earth;

    public Scenario()
    {
        API.Instance.LoadKernels(new DirectoryInfo("Data"));
        _earth = new CelestialBody(399);
        _geopotential = new GeopotentialGravitationalField(new FileInfo("Data/SolarSystem/EGM2008_to70_TideFree"));
    }

    [Benchmark(Description = "Spacecraft propagator")]
    public void Propagate()
    {
        var scenario = new IO.Astrodynamics.Tests.Mission.ScenarioTests();
        scenario.Propagate();
    }

    [Benchmark(Description = "Compute gravitational acceleration")]
    public void Gravity()
    {
        var sv = new StateVector(new Vector3(6800000.0-Random.Shared.NextDouble(), 0.0, 0.0), new Vector3(0.0, 8000.0-Random.Shared.NextDouble(), 0.0), _earth, DateTimeExtension.J2000, Frame.ICRF);
        _geopotential.ComputeGravitationalAcceleration(sv);
    }
}