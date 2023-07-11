// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Scenario
{
    private const int SITESIZE = 10;
    private const int CELESTIALBODIESIZE = 10;

    public readonly string Name;
    public Window Window;
    public Spacecraft Spacecraft;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SITESIZE)]
    public readonly Site[] Sites;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = CELESTIALBODIESIZE)]
    public readonly int[] AdditionalCelestialBodiesId;

    public string Error { get; } = string.Empty;

    public Scenario(string name, Window window) : this()
    {
        Name = name;
        Window = window;
        Sites = ArrayBuilder.ArrayOf<Site>(SITESIZE);
        AdditionalCelestialBodiesId = new int[CELESTIALBODIESIZE];
        Array.Fill(AdditionalCelestialBodiesId, -1);
    }
    
    public bool HasError()
    {
        return !string.IsNullOrEmpty(Error);
    }
}