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
    
    [Option('s', Description = "Is state vector")]
    public bool IsStateVector { get; set; }
    
    [Option('k', Description = "Is keplerian elements")]
    public bool IsKeplerian { get; set; }
    
    [Option('e', Description = "Is equinoctial elements")]
    public bool IsEquinoctial { get; set; }
        
    [Option('t', Description = "Is two lines elements")]
    public bool IsTLE { get; set; }
    
}