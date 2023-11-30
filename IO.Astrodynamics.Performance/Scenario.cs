// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using BenchmarkDotNet.Attributes;

namespace IO.Astrodynamics.Performance;

[MarkdownExporter]
[MemoryDiagnoser]
[SkewnessColumn]
[KurtosisColumn]
[StatisticalTestColumn]
[ShortRunJob]
public class Scenario
{
    [Benchmark(Description = "Spacecraft propagator")]   
    public void Propagate()
    {
        var scenario = new IO.Astrodynamics.Tests.Mission.ScenarioTests();
        scenario.Propagate();
    }
}