using System;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Scenario
{
    public string Name;
    public Window Window;
    public Spacecraft Spacecraft;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public Site[] Sites;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public Distance[] Distances;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public Occultation[] Occultations;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public CelestialBody[] CelestialBodies;
}