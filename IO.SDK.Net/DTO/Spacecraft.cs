using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Spacecraft
{
    internal const int FUELTANKSIZE = 5;
    internal const int ENGINESIZE = 5;
    internal const int INSTRUMENTSIZE = 5;
    internal const int PAYLOADSIZE = 5;
    internal const int ATTITUDESIZE = 10;
    internal const int MANEUVERSIZE = 10;

    public Spacecraft(int id, string name, double dryOperatingMass, double maximumOperatingMass,
        StateVector initialOrbitalParameter, string directoryPath)
    {
        Id = id;
        Name = name;
        DryOperatingMass = dryOperatingMass;
        MaximumOperatingMass = maximumOperatingMass;
        InitialOrbitalParameter = initialOrbitalParameter;
        DirectoryPath = directoryPath;

        FuelTanks = ArrayBuilder.ArrayOf<FuelTank>(FUELTANKSIZE);
        Engines = ArrayBuilder.ArrayOf<EngineDTO>(ENGINESIZE);
        Instruments = ArrayBuilder.ArrayOf<Instrument>(INSTRUMENTSIZE);
        Payloads = ArrayBuilder.ArrayOf<Payload>(PAYLOADSIZE);

        progradeAttitudes = ArrayBuilder.ArrayOf<ProgradeAttitude>(ATTITUDESIZE);
        retrogradeAttitudes = ArrayBuilder.ArrayOf<RetrogradeAttitude>(ATTITUDESIZE);
        zenithAttitudes = ArrayBuilder.ArrayOf<ZenithAttitude>(ATTITUDESIZE);
        nadirAttitudes = ArrayBuilder.ArrayOf<NadirAttitude>(ATTITUDESIZE);
        PointingToAttitudes = ArrayBuilder.ArrayOf<InstrumentPointingToAttitude>(ATTITUDESIZE);

        PerigeeHeightChangingManeuvers = ArrayBuilder.ArrayOf<PerigeeHeightChangingManeuver>(MANEUVERSIZE);
        ApogeeHeightChangingManeuvers = ArrayBuilder.ArrayOf<ApogeeHeightChangingManeuver>(MANEUVERSIZE);
        OrbitalPlaneChangingManeuvers = ArrayBuilder.ArrayOf<OrbitalPlaneChangingManeuver>(MANEUVERSIZE);
        CombinedManeuvers = ArrayBuilder.ArrayOf<CombinedManeuver>(MANEUVERSIZE);
        ApsidalAlignmentManeuvers = ArrayBuilder.ArrayOf<ApsidalAlignmentManeuver>(MANEUVERSIZE);
        PhasingManeuver = ArrayBuilder.ArrayOf<PhasingManeuver>(MANEUVERSIZE);

    }

    //Spacecraft structure
    public int Id;
    public string Name;
    public double DryOperatingMass;
    public double MaximumOperatingMass;
    public StateVector InitialOrbitalParameter;
    public string DirectoryPath;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FUELTANKSIZE)]
    public FuelTank[] FuelTanks;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public EngineDTO[] Engines;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public Instrument[] Instruments;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public Payload[] Payloads;

    //Spacecraft attitudes
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public ProgradeAttitude[] progradeAttitudes;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public RetrogradeAttitude[] retrogradeAttitudes;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public ZenithAttitude[] zenithAttitudes;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public NadirAttitude[] nadirAttitudes;

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
    public PhasingManeuver[] PhasingManeuver;
}