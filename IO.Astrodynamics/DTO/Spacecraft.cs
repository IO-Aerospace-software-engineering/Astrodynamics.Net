// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

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

        ProgradeAttitudes = ArrayBuilder.ArrayOf<ProgradeAttitude>(ATTITUDESIZE);
        RetrogradeAttitudes = ArrayBuilder.ArrayOf<RetrogradeAttitude>(ATTITUDESIZE);
        ZenithAttitudes = ArrayBuilder.ArrayOf<ZenithAttitude>(ATTITUDESIZE);
        NadirAttitudes = ArrayBuilder.ArrayOf<NadirAttitude>(ATTITUDESIZE);
        PointingToAttitudes = ArrayBuilder.ArrayOf<InstrumentPointingToAttitude>(ATTITUDESIZE);

        PerigeeHeightChangingManeuvers = ArrayBuilder.ArrayOf<PerigeeHeightChangingManeuver>(MANEUVERSIZE);
        ApogeeHeightChangingManeuvers = ArrayBuilder.ArrayOf<ApogeeHeightChangingManeuver>(MANEUVERSIZE);
        OrbitalPlaneChangingManeuvers = ArrayBuilder.ArrayOf<OrbitalPlaneChangingManeuver>(MANEUVERSIZE);
        CombinedManeuvers = ArrayBuilder.ArrayOf<CombinedManeuver>(MANEUVERSIZE);
        ApsidalAlignmentManeuvers = ArrayBuilder.ArrayOf<ApsidalAlignmentManeuver>(MANEUVERSIZE);
        PhasingManeuver = ArrayBuilder.ArrayOf<PhasingManeuver>(MANEUVERSIZE);
    }

    //Spacecraft structure
    public readonly int Id;
    public readonly string Name;
    public readonly double DryOperatingMass;
    public readonly double MaximumOperatingMass;
    public StateVector InitialOrbitalParameter;
    public readonly string DirectoryPath;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FUELTANKSIZE)]
    public readonly FuelTank[] FuelTanks;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public readonly EngineDTO[] Engines;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public readonly Instrument[] Instruments;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public readonly Payload[] Payloads;

    //Spacecraft attitudes
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly ProgradeAttitude[] ProgradeAttitudes;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly RetrogradeAttitude[] RetrogradeAttitudes;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly ZenithAttitude[] ZenithAttitudes;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly NadirAttitude[] NadirAttitudes;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly InstrumentPointingToAttitude[] PointingToAttitudes;

    //Spacecraft maneuvers
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly PerigeeHeightChangingManeuver[] PerigeeHeightChangingManeuvers;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly ApogeeHeightChangingManeuver[] ApogeeHeightChangingManeuvers;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly OrbitalPlaneChangingManeuver[] OrbitalPlaneChangingManeuvers;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly CombinedManeuver[] CombinedManeuvers;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly ApsidalAlignmentManeuver[] ApsidalAlignmentManeuvers;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public readonly PhasingManeuver[] PhasingManeuver;
}