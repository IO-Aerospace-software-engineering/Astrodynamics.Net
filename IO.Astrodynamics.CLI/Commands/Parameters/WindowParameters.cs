// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using Cocona;

namespace IO.Astrodynamics.CLI.Commands.Parameters;

public class WindowParameters : ICommandParameterSet
{
    public EpochParameters Begin { get; set; }
    public EpochParameters End { get; set; }
}