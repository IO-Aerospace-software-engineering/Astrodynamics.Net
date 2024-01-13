// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using Cocona;

namespace IO.Astrodynamics.CLI.Commands.Parameters;

public class WindowParameters : ICommandParameterSet
{
    [Argument(Description = "Start of search window")]
    public string Begin { get; set; }
    
    [Argument(Description = "End of search window")]
    public string End { get; set; }
}