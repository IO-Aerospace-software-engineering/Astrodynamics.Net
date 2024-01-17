using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cocona;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.CLI.Commands.Parameters;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.CLI.Commands;

public class TestsCommand
{
    public TestsCommand()
    {
    }

    [Command("tests", Description = "Compute orientations of given object")]
    public Task Tests(Window2Parameters windowParameters)
    {
        return Task.CompletedTask;
    }
}