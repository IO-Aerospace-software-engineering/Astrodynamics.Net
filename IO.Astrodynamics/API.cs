// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using IO.Astrodynamics.DTO;
using IO.Astrodynamics.Models.Frames;
using IO.Astrodynamics.Models.Maneuver;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.Time;
using AutoMapper;
using IO.Astrodynamics.Converters;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Math;
using ApsidalAlignmentManeuver = IO.Astrodynamics.DTO.ApsidalAlignmentManeuver;
using CelestialBody = IO.Astrodynamics.DTO.CelestialBody;
using CombinedManeuver = IO.Astrodynamics.DTO.CombinedManeuver;
using Launch = IO.Astrodynamics.DTO.Launch;
using PhasingManeuver = IO.Astrodynamics.DTO.PhasingManeuver;
using Quaternion = IO.Astrodynamics.Models.Math.Quaternion;
using Scenario = IO.Astrodynamics.DTO.Scenario;
using Window = IO.Astrodynamics.DTO.Window;

namespace IO.Astrodynamics;

/// <summary>
///     API to communicate with IO.Astrodynamics
/// </summary>
public class API
{
    private static bool _isResolverLoaded;
    private readonly IMapper _mapper;

    /// <summary>
    ///     Instantiate API
    /// </summary>
    public API()
    {
        _mapper = ProfilesConfiguration.Instance.Mapper;
        if (_isResolverLoaded) return;
        _isResolverLoaded = true;
        NativeLibrary.SetDllImportResolver(typeof(API).Assembly, Resolver);
    }

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string GetSpiceVersionProxy();

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void PropagateProxy([In] [Out] ref Scenario scenario);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void LaunchProxy([In] [Out] ref Launch launch);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void LoadKernelsProxy(string directoryPath);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string TDBToStringProxy(double secondsFromJ2000);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string UTCToStringProxy(double secondsFromJ2000);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void FindWindowsOnDistanceConstraintProxy(Window searchWindow, int observerId,
        int targetId, string constraint, double value, string aberration, double stepSize, [In] [Out] Window[] windows);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void FindWindowsOnOccultationConstraintProxy(Window searchWindow, int observerId,
        int targetId,
        string targetFrame, string targetShape, int frontBodyId, string frontFrame, string frontShape,
        string occultationType,
        string aberration, double stepSize, [In] [Out] Window[] windows);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void FindWindowsOnCoordinateConstraintProxy(Window searchWindow, int observerId, int targetId,
        string frame, string coordinateSystem, string coordinate,
        string relationalOperator, double value, double adjustValue, string aberration, double stepSize,
        [In] [Out] Window[] windows);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void FindWindowsOnIlluminationConstraintProxy(Window searchWindow, int observerId,
        string illuminationSource, int targetBody, string fixedFrame,
        Geodetic geodetic, string illuminationType, string relationalOperator, double value, double adjustValue,
        string aberration, double stepSize, string method, [In] [Out] Window[] windows);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void FindWindowsInFieldOfViewConstraintProxy(Window searchWindow, int observerId,
        int instrumentId, int targetId, string targetFrame, string targetShape, string aberration, double stepSize,
        [In] [Out] Window[] windows);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void ReadOrientationProxy(Window searchWindow, int spacecraftId, double tolerance,
        string frame, double stepSize, [In] [Out] StateOrientation[] stateOrientations);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void ReadEphemerisProxy(Window searchWindow, int observerId, int targetId,
        string frame, string aberration, double stepSize, [In] [Out] StateVector[] stateVectors);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern double ConvertUTCToTDBProxy(double utc);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern double ConvertTDBToUTCProxy(double tdb);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern bool
        WriteEphemerisProxy(string filePath, int objectId, StateVector[] stateVectors, uint size);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern CelestialBody GetCelestialBodyInfoProxy(int celestialBodyId);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern FrameTransformation TransformFrameProxy(string fromFrame, string toFrame, double epoch);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern StateVector ConvertEquinoctialElementsToStateVectorProxy(
        EquinoctialElements equinoctialElements);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern StateVector ConvertConicElementsToStateVectorProxy(ConicElements conicElements);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern RaDec ConvertStateVectorToEquatorialCoordinatesProxy(StateVector stateVector);

    private static IntPtr Resolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        var libHandle = IntPtr.Zero;

        if (libraryName != "IO.Astrodynamics") return libHandle;
        string sharedLibName = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            sharedLibName = "resources/IO.Astrodynamics.dll";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) sharedLibName = "resources/libIO.Astrodynamics.so";

        if (!string.IsNullOrEmpty(sharedLibName))
            NativeLibrary.TryLoad(sharedLibName, typeof(API).Assembly, DllImportSearchPath.AssemblyDirectory,
                out libHandle);
        else
            throw new PlatformNotSupportedException();

        return libHandle;
    }

    /// <summary>
    ///     Get spice toolkit version number
    /// </summary>
    /// <returns></returns>
    public string GetSpiceVersion()
    {
        return GetSpiceVersionProxy();
    }

    /// <summary>
    ///     Execute the scenario
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="outputDirectory"></param>
    public void PropagateScenario(Models.Mission.Scenario scenario, DirectoryInfo outputDirectory)
    {
        Scenario scenarioDto = new Scenario(scenario.Name,
            new Window(scenario.Window.StartDate.SecondsFromJ2000TDB(), scenario.Window.EndDate.SecondsFromJ2000TDB()));
        foreach (var site in scenario.Sites)
        {
            for (int j = 0; j < scenario.Sites.Count(); j++)
            {
                scenarioDto.Sites[j] = _mapper.Map<Site>(scenario.Sites.ElementAt(j));
                scenarioDto.Sites[j].DirectoryPath = outputDirectory.CreateSubdirectory("Sites").FullName;
            }
        }

        foreach (var spacecraft in scenario.Bodies.OfType<SpacecraftScenario>())
        {
            for (int j = 0; j < scenario.Bodies.OfType<CelestialBodyScenario>().Count(); j++)
            {
                scenarioDto.CelestialBodiesId[j] = scenario.Bodies.ElementAt(j).PhysicalBody.NaifId;
            }

            //Define parking orbit
            StateVector parkingOrbit = _mapper.Map<StateVector>(spacecraft.InitialOrbitalParameters.ToStateVector());

            //Create and configure spacecraft
            scenarioDto.Spacecraft = new Spacecraft(spacecraft.PhysicalBody.NaifId, spacecraft.PhysicalBody.Name,
                spacecraft.PhysicalBody.DryOperatingMass,
                spacecraft.PhysicalBody.MaximumOperatingMass, parkingOrbit,
                outputDirectory.CreateSubdirectory("Spacecrafts").FullName);
            for (int j = 0; j < spacecraft.FuelTanks.Count; j++)
            {
                var fuelTank = spacecraft.FuelTanks.ElementAt(j);
                scenarioDto.Spacecraft.FuelTanks[j] = new FuelTank(j + 1, capacity: fuelTank.FuelTank.Capacity,
                    quantity: fuelTank.Quantity, serialNumber: fuelTank.SerialNumber);
            }

            for (int j = 0; j < spacecraft.Engines.Count; j++)
            {
                var engine = spacecraft.Engines.ElementAt(j);
                scenarioDto.Spacecraft.Engines[j] = new EngineDTO(id: j + 1, name: engine.Engine.Name,
                    fuelFlow: engine.Engine.FuelFlow, serialNumber: engine.SerialNumber,
                    fuelTankSerialNumber: engine.FuelTank.SerialNumber, isp: engine.Engine.ISP);
            }

            for (int j = 0; j < spacecraft.Payloads.Count; j++)
            {
                var payload = spacecraft.Payloads.ElementAt(j);
                scenarioDto.Spacecraft.Payloads[j] = new Payload(payload.SerialNumber, payload.Name, payload.Mass);
            }

            for (int j = 0; j < spacecraft.Intruments.Count; j++)
            {
                var instrument = spacecraft.Intruments.ElementAt(j);
                scenarioDto.Spacecraft.Instruments[j] = new Instrument(instrument.Instrument.NaifId,
                    instrument.Instrument.Name, instrument.Instrument.Shape.GetDescription(),
                    _mapper.Map<Vector3D>(instrument.Orientation),
                    _mapper.Map<Vector3D>(instrument.Instrument.Boresight),
                    _mapper.Map<Vector3D>(instrument.Instrument.RefVector),
                    instrument.Instrument.FieldOfView, instrument.Instrument.CrossAngle);
            }

            //Build maneuvers
            var maneuver = spacecraft.StandbyManeuver;
            int order = 0;
            while (maneuver != null)
            {
                if (maneuver is PlaneAlignmentManeuver)
                {
                    StateVector target = _mapper.Map<StateVector>(maneuver.TargetOrbit.ToStateVector());
                    int idx = scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers[idx] = new OrbitalPlaneChangingManeuver(order,
                        maneuver.ManeuverHoldDuration.TotalSeconds,
                        maneuver.MinimumEpoch.SecondsFromJ2000TDB(), target);

                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is ApogeeHeightManeuver)
                {
                    int idx = scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers[
                            scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers.Count(x => x.ManeuverOrder > -1)] =
                        new ApogeeHeightChangingManeuver(order, maneuver.ManeuverHoldDuration.TotalSeconds,
                            maneuver.MinimumEpoch.SecondsFromJ2000TDB(),
                            (maneuver as ApogeeHeightManeuver).TargetApogee);

                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is IO.Astrodynamics.Models.Maneuver.ApsidalAlignmentManeuver)
                {
                    StateVector target = _mapper.Map<StateVector>(maneuver.TargetOrbit.ToStateVector());
                    int idx = scenarioDto.Spacecraft.ApsidalAlignmentManeuvers.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.ApsidalAlignmentManeuvers[
                            scenarioDto.Spacecraft.ApsidalAlignmentManeuvers.Count(x => x.ManeuverOrder > -1)] =
                        new ApsidalAlignmentManeuver(order, maneuver.ManeuverHoldDuration.TotalSeconds,
                            maneuver.MinimumEpoch.SecondsFromJ2000TDB(), target);

                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.ApsidalAlignmentManeuvers[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is IO.Astrodynamics.Models.Maneuver.CombinedManeuver)
                {
                    int idx = scenarioDto.Spacecraft.CombinedManeuvers.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.CombinedManeuvers[
                            scenarioDto.Spacecraft.CombinedManeuvers.Count(x => x.ManeuverOrder > -1)] =
                        new CombinedManeuver(order,
                            maneuver.ManeuverHoldDuration.TotalSeconds, maneuver.MinimumEpoch.SecondsFromJ2000TDB(),
                            maneuver.TargetOrbit.ApogeeVector().Magnitude(),
                            maneuver.TargetOrbit.Inclination());

                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.CombinedManeuvers[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is PerigeeHeightManeuver)
                {
                    int idx = scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers[
                            scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers.Count(x => x.ManeuverOrder > -1)] =
                        new PerigeeHeightChangingManeuver(order, maneuver.ManeuverHoldDuration.TotalSeconds,
                            maneuver.MinimumEpoch.SecondsFromJ2000TDB(),
                            maneuver.TargetOrbit.PerigeeVector().Magnitude());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is IO.Astrodynamics.Models.Maneuver.PhasingManeuver)
                {
                    StateVector target = _mapper.Map<StateVector>(maneuver.TargetOrbit.ToStateVector());
                    int idx = scenarioDto.Spacecraft.PhasingManeuver.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.PhasingManeuver[
                        scenarioDto.Spacecraft.PhasingManeuver.Count(x => x.ManeuverOrder > -1)] = new PhasingManeuver(
                        order,
                        maneuver.ManeuverHoldDuration.TotalSeconds, maneuver.MinimumEpoch.SecondsFromJ2000TDB(),
                        (int)(maneuver as IO.Astrodynamics.Models.Maneuver.PhasingManeuver).RevolutionNumber, target);
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.PhasingManeuver[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is Models.Maneuver.InstrumentPointingToAttitude)
                {
                    int idx = scenarioDto.Spacecraft.PointingToAttitudes.Count(x => x.ManeuverOrder > -1);
                    var instManeuver = maneuver as Models.Maneuver.InstrumentPointingToAttitude;
                    scenarioDto.Spacecraft.PointingToAttitudes[
                            scenarioDto.Spacecraft.PointingToAttitudes.Count(x => x.ManeuverOrder > -1)] =
                        new DTO.InstrumentPointingToAttitude(order, instManeuver.Instrument.Instrument.NaifId,
                            instManeuver.TargetId.NaifId,
                            instManeuver.ManeuverHoldDuration.TotalSeconds,
                            instManeuver.MinimumEpoch.SecondsFromJ2000TDB());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.PointingToAttitudes[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is Models.Maneuver.ProgradeAttitude)
                {
                    int idx = scenarioDto.Spacecraft.ProgradeAttitudes.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.ProgradeAttitudes[
                            scenarioDto.Spacecraft.ProgradeAttitudes.Count(x => x.ManeuverOrder > -1)] =
                        new DTO.ProgradeAttitude(order, maneuver.ManeuverHoldDuration.TotalSeconds,
                            maneuver.MinimumEpoch.SecondsFromJ2000TDB());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.ProgradeAttitudes[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is Models.Maneuver.RetrogradeAttitude)
                {
                    int idx = scenarioDto.Spacecraft.RetrogradeAttitudes.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.RetrogradeAttitudes[
                            scenarioDto.Spacecraft.RetrogradeAttitudes.Count(x => x.ManeuverOrder > -1)] =
                        new DTO.RetrogradeAttitude(order, maneuver.ManeuverHoldDuration.TotalSeconds,
                            maneuver.MinimumEpoch.SecondsFromJ2000TDB());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.RetrogradeAttitudes[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is Models.Maneuver.NadirAttitude)
                {
                    int idx = scenarioDto.Spacecraft.NadirAttitudes.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.NadirAttitudes[
                            scenarioDto.Spacecraft.NadirAttitudes.Count(x => x.ManeuverOrder > -1)] =
                        new DTO.NadirAttitude(order, maneuver.ManeuverHoldDuration.TotalSeconds,
                            maneuver.MinimumEpoch.SecondsFromJ2000TDB());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.NadirAttitudes[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is Models.Maneuver.ZenithAttitude)
                {
                    int idx = scenarioDto.Spacecraft.ZenithAttitudes.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.ZenithAttitudes[
                            scenarioDto.Spacecraft.ZenithAttitudes.Count(x => x.ManeuverOrder > -1)] =
                        new DTO.ZenithAttitude(order, maneuver.ManeuverHoldDuration.TotalSeconds,
                            maneuver.MinimumEpoch.SecondsFromJ2000TDB());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.ZenithAttitudes[idx].Engines[k] =
                            maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }

                maneuver = maneuver.NextManeuver;
                order++;
            }

            PropagateProxy(ref scenarioDto);
            //Todo load kernels

            foreach (var maneuverResult in scenarioDto.Spacecraft.CombinedManeuvers.Where(x => x.ManeuverOrder > -1))
            {
                var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                mnv.AttitudeWindow = _mapper.Map<Models.Time.Window>(maneuverResult.AttitudeWindow);
                mnv.ThrustWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ThrustWindow);
                mnv.ManeuverWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ManeuverWindow);
                mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                mnv.FuelBurned = maneuverResult.FuelBurned;
            }

            foreach (var maneuverResult in scenarioDto.Spacecraft.PhasingManeuver.Where(x => x.ManeuverOrder > -1))
            {
                var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                mnv.AttitudeWindow = _mapper.Map<Models.Time.Window>(maneuverResult.AttitudeWindow);
                mnv.ThrustWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ThrustWindow);
                mnv.ManeuverWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ManeuverWindow);
                mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                mnv.FuelBurned = maneuverResult.FuelBurned;
            }

            foreach (var maneuverResult in scenarioDto.Spacecraft.ApsidalAlignmentManeuvers.Where(x =>
                         x.ManeuverOrder > -1))
            {
                var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                mnv.AttitudeWindow = _mapper.Map<Models.Time.Window>(maneuverResult.AttitudeWindow);
                mnv.ThrustWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ThrustWindow);
                mnv.ManeuverWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ManeuverWindow);
                mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                mnv.FuelBurned = maneuverResult.FuelBurned;
            }

            foreach (var maneuverResult in scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers.Where(x =>
                         x.ManeuverOrder > -1))
            {
                var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                mnv.AttitudeWindow = _mapper.Map<Models.Time.Window>(maneuverResult.AttitudeWindow);
                mnv.ThrustWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ThrustWindow);
                mnv.ManeuverWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ManeuverWindow);
                mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                mnv.FuelBurned = maneuverResult.FuelBurned;
            }

            foreach (var maneuverResult in scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers.Where(x =>
                         x.ManeuverOrder > -1))
            {
                var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                mnv.AttitudeWindow = _mapper.Map<Models.Time.Window>(maneuverResult.AttitudeWindow);
                mnv.ThrustWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ThrustWindow);
                mnv.ManeuverWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ManeuverWindow);
                mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                mnv.FuelBurned = maneuverResult.FuelBurned;
            }

            foreach (var maneuverResult in scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers.Where(x =>
                         x.ManeuverOrder > -1))
            {
                var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                mnv.AttitudeWindow = _mapper.Map<Models.Time.Window>(maneuverResult.AttitudeWindow);
                mnv.ThrustWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ThrustWindow);
                mnv.ManeuverWindow = _mapper.Map<Models.Time.Window>(maneuverResult.ManeuverWindow);
                mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                mnv.FuelBurned = maneuverResult.FuelBurned;
            }
        }
    }

    /// <summary>
    ///     Load generic kernel at given path
    /// </summary>
    /// <param name="path">Path where kernels are located. This could be a file path or a directory path</param>
    public void LoadKernels(FileSystemInfo path)
    {
        LoadKernelsProxy(path.FullName);
    }

    /// <summary>
    ///     Find launch windows
    /// </summary>
    /// <param name="launch"></param>
    /// <param name="window"></param>
    /// <param name="outputDirectory"></param>
    public IEnumerable<LaunchWindow> FindLaunchWindows(IO.Astrodynamics.Models.Maneuver.Launch launch,
        in Models.Time.Window window, DirectoryInfo outputDirectory)
    {
        //Convert data
        Launch launchDto = _mapper.Map<Launch>(launch);
        launchDto.Window = _mapper.Map<Models.Time.Window, Window>(window);
        launchDto.LaunchSite.DirectoryPath = outputDirectory.CreateSubdirectory("Sites").FullName;
        launchDto.RecoverySite.DirectoryPath = outputDirectory.CreateSubdirectory("Sites").FullName;

        //Execute request
        LaunchProxy(ref launchDto);

        //Filter result
        var windows = launchDto.Windows.Where(x => x.Start != 0 && x.End != 0).ToArray();

        //Build result 
        List<LaunchWindow> launchWindows = new List<LaunchWindow>();

        for (int i = 0; i < windows.Count(); i++)
        {
            launchWindows.Add(new LaunchWindow(_mapper.Map<Window, Models.Time.Window>(windows[i]),
                launchDto.InertialInsertionVelocity[i], launchDto.NonInertialInsertionVelocity[i],
                launchDto.InertialAzimuth[i], launchDto.NonInertialAzimuth[i]));
        }

        return launchWindows;
    }

    /// <summary>
    ///     Find time windows based on distance constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observer"></param>
    /// <param name="target"></param>
    /// <param name="relationalOperator"></param>
    /// <param name="value"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public IEnumerable<Models.Time.Window> FindWindowsOnDistanceConstraint(Models.Time.Window searchWindow, INaifObject observer,
        INaifObject target, RelationnalOperator relationalOperator, double value, Aberration aberration, TimeSpan stepSize)
    {
        var windows = new Window[1000];
        for (var i = 0; i < 1000; i++)
        {
            windows[i] = new Window(double.NaN, double.NaN);
        }

        FindWindowsOnDistanceConstraintProxy(_mapper.Map<Window>(searchWindow), observer.NaifId, target.NaifId, relationalOperator.GetDescription(),
            value,
            aberration.GetDescription(),
            stepSize.TotalSeconds, windows);
        return _mapper.Map<Models.Time.Window[]>(windows.Where(x => !double.IsNaN(x.Start)));
    }

    /// <summary>
    ///     Find time windows based on occultation constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="target"></param>
    /// <param name="targetFrame"></param>
    /// <param name="targetShape"></param>
    /// <param name="frontBody"></param>
    /// <param name="frontFrame"></param>
    /// <param name="frontShape"></param>
    /// <param name="occultationType"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    public IEnumerable<Models.Time.Window> FindWindowsOnOccultationConstraint(Models.Time.Window searchWindow, INaifObject observer, INaifObject target,
        ShapeType targetShape, INaifObject frontBody, ShapeType frontShape, OccultationType occultationType, Aberration aberration, TimeSpan stepSize)
    {
        string frontFrame = frontShape == ShapeType.Ellipsoid ? (frontBody as CelestialBodyScenario)?.Frame.Name : string.Empty;
        string targetFrame = targetShape == ShapeType.Ellipsoid ? (target as CelestialBodyScenario)?.Frame.Name : String.Empty;
        var windows = new Window[1000];
        for (var i = 0; i < 1000; i++)
        {
            windows[i] = new Window(double.NaN, double.NaN);
        }

        FindWindowsOnOccultationConstraintProxy(_mapper.Map<Window>(searchWindow), observer.NaifId, target.NaifId, targetFrame, targetShape.GetDescription(),
            frontBody.NaifId, frontFrame, frontShape.GetDescription(), occultationType.GetDescription(), aberration.GetDescription(), stepSize.TotalSeconds, windows);
        return _mapper.Map<Models.Time.Window[]>(windows.Where(x => !double.IsNaN(x.Start)));
    }

    /// <summary>
    ///     Find time windows based on coordinate constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="frame"></param>
    /// <param name="coordinateSystem"></param>
    /// <param name="coordinate"></param>
    /// <param name="relationalOperator"></param>
    /// <param name="value"></param>
    /// <param name="adjustValue"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    public IEnumerable<Models.Time.Window> FindWindowsOnCoordinateConstraint(Models.Time.Window searchWindow, INaifObject observer, INaifObject target,
        Frame frame, CoordinateSystem coordinateSystem, Coordinate coordinate,
        RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration,
        TimeSpan stepSize)
    {
        //todo remove frame parameters from this and others constraints finder
        var windows = new Window[1000];
        for (var i = 0; i < 1000; i++)
        {
            windows[i] = new Window(double.NaN, double.NaN);
        }

        FindWindowsOnCoordinateConstraintProxy(_mapper.Map<Window>(searchWindow), observer.NaifId, target.NaifId, frame.Name, coordinateSystem.GetDescription(),
            coordinate.GetDescription(), relationalOperator.GetDescription(), value, adjustValue, aberration.GetDescription(), stepSize.TotalSeconds, windows);
        return _mapper.Map<Models.Time.Window[]>(windows.Where(x => !double.IsNaN(x.Start)));
    }

    /// <summary>
    ///     Find time windows based on illumination constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="illuminationSource"></param>
    /// <param name="observer"></param>
    /// <param name="targetBody"></param>
    /// <param name="fixedFrame"></param>
    /// <param name="geodetic"></param>
    /// <param name="illuminationType"></param>
    /// <param name="relationalOperator"></param>
    /// <param name="value"></param>
    /// <param name="adjustValue"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public IEnumerable<Models.Time.Window> FindWindowsOnIlluminationConstraint(Models.Time.Window searchWindow, INaifObject observer, INaifObject targetBody, Frame fixedFrame,
        Geodetic geodetic, IlluminationAngle illuminationType, RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration, TimeSpan stepSize,
        string illuminationSource = "SUN", string method = "Ellipsoid")
    {
        if (fixedFrame == null) throw new ArgumentNullException(nameof(fixedFrame));
        if (illuminationSource == null) throw new ArgumentNullException(nameof(illuminationSource));
        if (method == null) throw new ArgumentNullException(nameof(method));
        var windows = new Window[1000];
        for (var i = 0; i < 1000; i++)
        {
            windows[i] = new Window(double.NaN, double.NaN);
        }

        FindWindowsOnIlluminationConstraintProxy(_mapper.Map<Window>(searchWindow), observer.NaifId, illuminationSource, targetBody.NaifId, fixedFrame.Name,
            geodetic, illuminationType.GetDescription(), relationalOperator.GetDescription(), value, adjustValue,
            aberration.GetDescription(), stepSize.TotalSeconds,
            method, windows);
        return _mapper.Map<Models.Time.Window[]>(windows.Where(x => !double.IsNaN(x.Start)));
    }

    /// <summary>
    ///     Find time window when a target is in instrument's field of view
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="instrumentId"></param>
    /// <param name="targetId"></param>
    /// <param name="targetFrame"></param>
    /// <param name="targetShape"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public IEnumerable<Models.Time.Window> FindWindowsInFieldOfViewConstraint(Models.Time.Window searchWindow, INaifObject observerId, Models.Body.Spacecraft.Instrument instrument,
        INaifObject targetId, Frame targetFrame, ShapeType targetShape, Aberration aberration, TimeSpan stepSize)
    {
        if (targetFrame == null) throw new ArgumentNullException(nameof(targetFrame));
        var windows = new Window[1000];
        for (var i = 0; i < 1000; i++)
        {
            windows[i] = new Window(double.NaN, double.NaN);
        }

        var searchWindowDto = _mapper.Map<Window>(searchWindow);

        FindWindowsInFieldOfViewConstraintProxy(searchWindowDto, observerId.NaifId, observerId.NaifId * 1000 - instrument.NaifId, targetId.NaifId, targetFrame.Name,
            targetShape.GetDescription(), aberration.GetDescription(), stepSize.TotalSeconds, windows);

        return _mapper.Map<IEnumerable<Models.Time.Window>>(windows.Where(x => !double.IsNaN(x.Start)).ToArray());
    }

    /// <summary>
    ///     Read object ephemeris for a given period
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="target"></param>
    /// <param name="frame"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    public IEnumerable<Models.OrbitalParameters.OrbitalParameters> ReadEphemeris(Models.Time.Window searchWindow, CelestialBodyScenario observer, ILocalizable target, Frame frame,
        Aberration aberration, TimeSpan stepSize)
    {
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        var stateVectors = new StateVector[10000];
        ReadEphemerisProxy(_mapper.Map<Window>(searchWindow), observer.PhysicalBody.NaifId, target.NaifId, frame.Name, aberration.GetDescription(), stepSize.TotalSeconds,
            stateVectors);
        return stateVectors.Select(x =>
            new Models.OrbitalParameters.StateVector(_mapper.Map<Vector3>(x.Position), _mapper.Map<Vector3>(x.Velocity), observer, DateTimeExtension.CreateTDB(x.Epoch), frame));
    }

    /// <summary>
    ///     Read spacecraft orientation for a given period
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="spacecraft"></param>
    /// <param name="tolerance"></param>
    /// <param name="referenceFrame"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public IEnumerable<Models.OrbitalParameters.StateOrientation> ReadOrientation(Models.Time.Window searchWindow, SpacecraftScenario spacecraft, TimeSpan tolerance,
        Frame referenceFrame, TimeSpan stepSize)
    {
        if (referenceFrame == null) throw new ArgumentNullException(nameof(referenceFrame));
        var stateOrientations = new StateOrientation[10000];
        ReadOrientationProxy(_mapper.Map<Window>(searchWindow), spacecraft.PhysicalBody.NaifId, tolerance.TotalSeconds, referenceFrame.Name, stepSize.TotalSeconds,
            stateOrientations);
        return stateOrientations.Select(x => new Models.OrbitalParameters.StateOrientation(_mapper.Map<Quaternion>(x.Rotation), _mapper.Map<Vector3>(x.AngularVelocity),
            DateTimeExtension.CreateTDB(x.Epoch), referenceFrame));
    }

    /// <summary>
    ///     Write ephemeris file
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="naifObject"></param>
    /// <param name="stateVectors"></param>
    /// <returns></returns>
    public bool WriteEphemeris(FileInfo filePath, INaifObject naifObject, IEnumerable<Models.OrbitalParameters.StateVector> stateVectors)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        if (stateVectors == null) throw new ArgumentNullException(nameof(stateVectors));
        var enumerable = stateVectors as Models.OrbitalParameters.StateVector[] ?? stateVectors.ToArray();
        if (!enumerable.Any())
            throw new ArgumentException("Value cannot be an empty collection.", nameof(stateVectors));
        return WriteEphemerisProxy(filePath.FullName, naifObject.NaifId, _mapper.Map<StateVector[]>(stateVectors), (uint)enumerable.Count());
    }

    /// <summary>
    ///     Get celestial body information like radius, GM, name, associated frame, ...
    /// </summary>
    /// <param name="celestialBody"></param>
    /// <returns></returns>
    public CelestialBody GetCelestialBodyInfo(Models.Body.CelestialBody celestialBody)
    {
        return GetCelestialBodyInfoProxy(celestialBody.NaifId);
    }

    /// <summary>
    /// Transform a frame to another
    /// </summary>
    /// <param name="fromFrame"></param>
    /// <param name="toFrame"></param>
    /// <param name="epoch"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Models.OrbitalParameters.StateOrientation TransformFrame(Frame fromFrame, Frame toFrame, DateTime epoch)
    {
        if (fromFrame == null) throw new ArgumentNullException(nameof(fromFrame));
        if (toFrame == null) throw new ArgumentNullException(nameof(toFrame));
        var res = TransformFrameProxy(fromFrame.Name, toFrame.Name, epoch.ToTDB().SecondsFromJ2000TDB());
        return new Models.OrbitalParameters.StateOrientation(
            new Quaternion(res.Rotation.W, res.Rotation.X, res.Rotation.Y, res.Rotation.Z),
            new Vector3(res.AngularVelocity.X, res.AngularVelocity.Y, res.AngularVelocity.Z), epoch, fromFrame);
    }
}