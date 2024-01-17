// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using Cocona;

namespace IO.Astrodynamics.CLI.Commands.Parameters;

public class OrbitalParameters : ICommandParameterSet
{
    public EpochParameters EpochParameter { get; set; }
    
    [Argument(Description = "Center of motion")]
    public int CenterOfMotionId { get; set; }
    
    [Argument(Description = "Orbital parameters")]
    public string OrbitalParametersValues { get; set; }
    
    [Argument(Description = "Frame")]
    public string Frame { get; set; }
    
    [Option('s', Description = "Input represents a state vector")]
    public bool FromStateVector { get; set; }
    
    [Option('k', Description = "Input represents keplerian elements")]
    public bool FromKeplerian { get; set; }
    
    [Option('q', Description = "Input represents equinoctial elements")]
    public bool FromEquinoctial { get; set; }
        
    [Option('t', Description = "Input represents two lines elements")]
    public bool FromTLE { get; set; }
    
}