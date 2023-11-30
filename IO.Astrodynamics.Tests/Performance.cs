// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using BenchmarkDotNet.Running;
using IO.Astrodynamics.Tests.Mission;
using Xunit;

namespace IO.Astrodynamics.Tests;

public class Performance
{
    [Fact]
    public void Scenario()
    {
        var result = BenchmarkRunner.Run<ScenarioTests>();
        var duration = TimeSpan.FromMicroseconds(result.Reports[0].ResultStatistics.Percentiles.P95 * 1E-03);
    }
}