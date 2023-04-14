using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Scenario
{
    private const int SITESIZE = 2;
    private const int CELESTIALBODIESIZE = 10;
    public Scenario(string name, Window window) : this()
    {
        Name = name;
        Window = window;
        Sites = ArrayBuilder.ArrayOf<Site>(SITESIZE);
        CelestialBodies = ArrayBuilder.ArrayOf<CelestialBody>(CELESTIALBODIESIZE);
    }

    public string Name;
    public Window Window;
    public Launch Launch;
    public Spacecraft Spacecraft;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SITESIZE)]
    public Site[] Sites;
    // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    // public Distance[] Distances;
    // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    // public Occultation[] Occultations;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = CELESTIALBODIESIZE)]
    public CelestialBody[] CelestialBodies;
}