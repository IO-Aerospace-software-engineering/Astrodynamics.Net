﻿using IO.Astrodynamics.CLI.Commands;

namespace IO.Astrodynamics.CLI;
using Cocona;

class Program
{
    static void Main(string[] args)
    {
        var builder = CoconaApp.CreateBuilder();

        var app = builder.Build();
        app.AddCommands<EphemerisCommand>();
        app.AddCommands<OrientationCommand>();
        app.AddCommands<TimeConverterCommand>();
        app.AddCommands<OrbitalParametersConverterCommand>();
        app.AddCommands<GeometryFinderCommand>();
        app.AddCommands<BodyInformationCommand>();
        app.Run();
    }
}