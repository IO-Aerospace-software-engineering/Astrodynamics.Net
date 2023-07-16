// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Collections.Generic;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Mission;

public class ScenarioSummary
{
    public Window Window { get; }

    private readonly HashSet<SpacecraftSummary> _spacecraftSummaries = new HashSet<SpacecraftSummary>();
    public IReadOnlyCollection<SpacecraftSummary> SpacecraftSummaries => _spacecraftSummaries;

    internal ScenarioSummary(Window window)
    {
        Window = window;
    }

    internal void AddSpacecraftSummary(SpacecraftSummary summary)
    {
        _spacecraftSummaries.Add(summary);
    }
}