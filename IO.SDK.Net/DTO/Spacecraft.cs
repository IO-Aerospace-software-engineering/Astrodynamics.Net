using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Spacecraft
{
    //Spacecraft structure
    public int Id;
    public string Name;
    public double DryOperatingMass;
    public double MaximumOperatingMass;
    public StateVector InitialOrbitalParameter;

     [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
     public FuelTank[] FuelTank;

     [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
     public EngineDTO[] Engines;

     [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
     public Instrument[] Instruments;

     [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
     public Payload[] Payloads;

    //Spacecraft attitudes
     [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
     public Attitude[] Attitudes;

     [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
     public InstrumentPointingToAttitude[] PointingToAttitudes;

    //Spacecraft maneuvers
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public PerigeeHeightChangingManeuver[] PerigeeHeightChangingManeuvers;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public ApogeeHeightChangingManeuver[] ApogeeHeightChangingManeuvers;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public OrbitalPlaneChangingManeuver[] OrbitalPlaneChangingManeuvers;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public CombinedManeuver[] CombinedManeuvers;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public ApsidalAlignmentManeuver[] ApsidalAlignmentManeuvers;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public PhasingManeuver[] PhasingManeuverDto;
    
    public Launch launch;

//Spacecraft states
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
    public StateVector[] StateVectors;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    public StateOrientation[] StateOrientations;
}