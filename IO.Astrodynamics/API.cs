// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using AutoMapper;
using IO.Astrodynamics.Converters;
using IO.Astrodynamics.DTO;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using IO.Astrodynamics.Time;
using ApsidalAlignmentManeuver = IO.Astrodynamics.Maneuver.ApsidalAlignmentManeuver;
using CelestialBody = IO.Astrodynamics.DTO.CelestialBody;
using CombinedManeuver = IO.Astrodynamics.Maneuver.CombinedManeuver;
using Instrument = IO.Astrodynamics.Body.Spacecraft.Instrument;
using InstrumentPointingToAttitude = IO.Astrodynamics.Maneuver.InstrumentPointingToAttitude;
using Launch = IO.Astrodynamics.DTO.Launch;
using NadirAttitude = IO.Astrodynamics.Maneuver.NadirAttitude;
using PhasingManeuver = IO.Astrodynamics.Maneuver.PhasingManeuver;
using ProgradeAttitude = IO.Astrodynamics.Maneuver.ProgradeAttitude;
using Quaternion = IO.Astrodynamics.Math.Quaternion;
using RetrogradeAttitude = IO.Astrodynamics.Maneuver.RetrogradeAttitude;
using Scenario = IO.Astrodynamics.DTO.Scenario;
using Spacecraft = IO.Astrodynamics.Body.Spacecraft.Spacecraft;
using StateOrientation = IO.Astrodynamics.DTO.StateOrientation;
using StateVector = IO.Astrodynamics.DTO.StateVector;
using Window = IO.Astrodynamics.DTO.Window;
using ZenithAttitude = IO.Astrodynamics.Maneuver.ZenithAttitude;

namespace IO.Astrodynamics;

/// <summary>
///     API to communicate with IO.Astrodynamics
/// </summary>
public class API
{
    private readonly IMapper _mapper;

    /// <summary>
    ///     Instantiate API
    /// </summary>
    private API()
    {
        _mapper = ProfilesConfiguration.Instance.Mapper;
        NativeLibrary.SetDllImportResolver(typeof(API).Assembly, Resolver);
    }

    public static API Instance { get; } = new();

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string GetSpiceVersionProxy();

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void PropagateSpacecraftProxy([In] [Out] ref Scenario scenario);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void LaunchProxy([In] [Out] ref Launch launch);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern bool LoadKernelsProxy(string directoryPath);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern bool UnloadKernelsProxy(string directoryPath);

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
        Planetodetic geodetic, string illuminationType, string relationalOperator, double value, double adjustValue,
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
    private static extern bool
        WriteEphemerisProxy(string filePath, int objectId, StateVector[] stateVectors, uint size);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern CelestialBody GetCelestialBodyInfoProxy(int celestialBodyId);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern FrameTransformation TransformFrameProxy(string fromFrame, string toFrame, double epoch);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern StateVector ReadEphemerisAtGivenEpochProxy(double epoch, int observerId, int targetId,
        string frame, string aberration);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern StateVector ConvertTLEToStateVectorProxy(string line1, string line2, string line3,
        double epoch);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern TLEElements GetTLEElementsProxy(string line1, string line2, string line3);

    [DllImport(@"IO.Astrodynamics", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void PropagateSiteProxy(DTO.Window window, [In] [Out] ref DTO.Site site);

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

    //Use the same lock for all cspice calls because it doesn't support multithreading.
    private static object lockObject = new object();

    /// <summary>
    ///     Get spice toolkit version number
    /// </summary>
    /// <returns></returns>
    public string GetSpiceVersion()
    {
        lock (lockObject)
        {
            return GetSpiceVersionProxy();
        }
    }

    public void PropagateSite(in Time.Window window, Surface.Site site, DirectoryInfo outputDirectory)
    {
        lock (lockObject)
        {
            var siteDto = _mapper.Map<DTO.Site>(site);
            siteDto.DirectoryPath = outputDirectory.CreateSubdirectory("Sites").FullName;
            var windowDto = _mapper.Map<Window>(window);
            PropagateSiteProxy(windowDto, ref siteDto);
            if (siteDto.HasError())
            {
                throw new InvalidOperationException($"Site can't be propagated : {siteDto.Error}");
            }

            LoadKernels(outputDirectory.CreateSubdirectory("Sites"));
        }
    }

    /// <summary>
    ///  Execute the scenario.
    ///  Kernels stay loaded after this execution
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="siteDirectoryInfo"></param>
    /// <param name="spacecraftDirectoryInfo"></param>
    public void PropagateScenario(Mission.Scenario scenario, DirectoryInfo siteDirectoryInfo, DirectoryInfo spacecraftDirectoryInfo)
    {
        if (scenario == null) throw new ArgumentNullException(nameof(scenario));
        if (siteDirectoryInfo == null) throw new ArgumentNullException(nameof(siteDirectoryInfo));
        if (spacecraftDirectoryInfo == null) throw new ArgumentNullException(nameof(spacecraftDirectoryInfo));
        lock (lockObject)
        {
            API.Instance.UnloadKernels(siteDirectoryInfo);
            API.Instance.UnloadKernels(spacecraftDirectoryInfo);

            //Clean outputs
            siteDirectoryInfo.Delete(true);
            spacecraftDirectoryInfo.Delete(true);

            Scenario scenarioDto = new Scenario(scenario.Name,
                new Window(scenario.Window.StartDate.SecondsFromJ2000TDB(),
                    scenario.Window.EndDate.SecondsFromJ2000TDB()));
            for (int j = 0; j < scenario.Sites.Count; j++)
            {
                scenarioDto.Sites[j] = _mapper.Map<Site>(scenario.Sites.ElementAt(j));
                scenarioDto.Sites[j].DirectoryPath = siteDirectoryInfo.FullName;
            }

            foreach (var spacecraft in scenario.Spacecrafts)
            {
                //Reset for each propagation
                for (int j = 0; j < scenarioDto.AdditionalCelestialBodiesId.Length; j++)
                {
                    scenarioDto.AdditionalCelestialBodiesId[j] = -1;
                }

                //add additional celestial bodies
                foreach (var celestialBody in scenario.AdditionalCelstialBodies)
                {
                    scenarioDto.AdditionalCelestialBodiesId[scenarioDto.AdditionalCelestialBodiesId.Count(x => x > -1)] = celestialBody.NaifId;
                }

                //Define parking orbit
                StateVector parkingOrbit =
                    _mapper.Map<StateVector>(spacecraft.InitialOrbitalParameters.ToStateVector());

                //Create and configure spacecraft
                scenarioDto.Spacecraft = new DTO.Spacecraft(spacecraft.NaifId, spacecraft.Name, spacecraft.DryOperatingMass, spacecraft.MaximumOperatingMass, parkingOrbit,
                    spacecraftDirectoryInfo.FullName);

                for (int j = 0; j < spacecraft.FuelTanks.Count; j++)
                {
                    var fuelTank = spacecraft.FuelTanks.ElementAt(j);
                    scenarioDto.Spacecraft.FuelTanks[j] = new FuelTank(j + 1, capacity: fuelTank.Capacity,
                        quantity: fuelTank.InitialQuantity, serialNumber: fuelTank.SerialNumber);
                }

                for (int j = 0; j < spacecraft.Engines.Count; j++)
                {
                    var engine = spacecraft.Engines.ElementAt(j);
                    scenarioDto.Spacecraft.Engines[j] = new EngineDTO(id: j + 1, name: engine.Name,
                        fuelFlow: engine.FuelFlow, serialNumber: engine.SerialNumber,
                        fuelTankSerialNumber: engine.FuelTank.SerialNumber, isp: engine.ISP);
                }

                for (int j = 0; j < spacecraft.Payloads.Count; j++)
                {
                    var payload = spacecraft.Payloads.ElementAt(j);
                    scenarioDto.Spacecraft.Payloads[j] = new Payload(payload.SerialNumber, payload.Name, payload.Mass);
                }

                for (int j = 0; j < spacecraft.Intruments.Count; j++)
                {
                    var instrument = spacecraft.Intruments.ElementAt(j);
                    scenarioDto.Spacecraft.Instruments[j] = new DTO.Instrument(instrument.NaifId,
                        instrument.Name, instrument.Shape.GetDescription(),
                        _mapper.Map<Vector3D>(instrument.Orientation),
                        _mapper.Map<Vector3D>(instrument.Boresight),
                        _mapper.Map<Vector3D>(instrument.RefVector),
                        instrument.FieldOfView, instrument.CrossAngle);
                }

                //Build maneuvers
                var maneuver = spacecraft.StandbyManeuver;
                int order = 0;
                while (maneuver != null)
                {
                    switch (maneuver)
                    {
                        case PlaneAlignmentManeuver:
                        {
                            StateVector target = _mapper.Map<StateVector>(maneuver.TargetOrbit.ToStateVector());
                            int idx = scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers.Count(x => x.ManeuverOrder > -1);
                            scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers[idx] = new OrbitalPlaneChangingManeuver(
                                order,
                                maneuver.ManeuverHoldDuration.TotalSeconds,
                                maneuver.MinimumEpoch.SecondsFromJ2000TDB(), target);

                            //Add engines
                            for (int k = 0; k < maneuver.Engines.Count; k++)
                            {
                                scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers[idx].Engines[k] =
                                    maneuver.Engines.ElementAt(k).SerialNumber;
                            }

                            break;
                        }
                        case ApogeeHeightManeuver heightManeuver:
                        {
                            int idx = scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers.Count(x => x.ManeuverOrder > -1);
                            scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers[
                                    scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers.Count(x => x.ManeuverOrder > -1)] =
                                new ApogeeHeightChangingManeuver(order, heightManeuver.ManeuverHoldDuration.TotalSeconds,
                                    heightManeuver.MinimumEpoch.SecondsFromJ2000TDB(),
                                    heightManeuver.TargetApogee);

                            //Add engines
                            for (int k = 0; k < heightManeuver.Engines.Count; k++)
                            {
                                scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers[idx].Engines[k] =
                                    heightManeuver.Engines.ElementAt(k).SerialNumber;
                            }

                            break;
                        }
                        case ApsidalAlignmentManeuver:
                        {
                            StateVector target = _mapper.Map<StateVector>(maneuver.TargetOrbit.ToStateVector());
                            int idx = scenarioDto.Spacecraft.ApsidalAlignmentManeuvers.Count(x => x.ManeuverOrder > -1);
                            scenarioDto.Spacecraft.ApsidalAlignmentManeuvers[
                                    scenarioDto.Spacecraft.ApsidalAlignmentManeuvers.Count(x => x.ManeuverOrder > -1)] =
                                new DTO.ApsidalAlignmentManeuver(order, maneuver.ManeuverHoldDuration.TotalSeconds,
                                    maneuver.MinimumEpoch.SecondsFromJ2000TDB(), target);

                            //Add engines
                            for (int k = 0; k < maneuver.Engines.Count; k++)
                            {
                                scenarioDto.Spacecraft.ApsidalAlignmentManeuvers[idx].Engines[k] =
                                    maneuver.Engines.ElementAt(k).SerialNumber;
                            }

                            break;
                        }
                        case CombinedManeuver combinedManeuver:
                        {
                            int idx = scenarioDto.Spacecraft.CombinedManeuvers.Count(x => x.ManeuverOrder > -1);
                            scenarioDto.Spacecraft.CombinedManeuvers[
                                    scenarioDto.Spacecraft.CombinedManeuvers.Count(x => x.ManeuverOrder > -1)] =
                                new DTO.CombinedManeuver(order,
                                    combinedManeuver.ManeuverHoldDuration.TotalSeconds, combinedManeuver.MinimumEpoch.SecondsFromJ2000TDB(),
                                    combinedManeuver.TargetPerigeeHeight,
                                    combinedManeuver.TargetInclination);

                            //Add engines
                            for (int k = 0; k < combinedManeuver.Engines.Count; k++)
                            {
                                scenarioDto.Spacecraft.CombinedManeuvers[idx].Engines[k] =
                                    combinedManeuver.Engines.ElementAt(k).SerialNumber;
                            }

                            break;
                        }
                        case PerigeeHeightManeuver perigeeHeightManeuver:
                        {
                            int idx = scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers.Count(x =>
                                x.ManeuverOrder > -1);
                            scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers[
                                    scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers.Count(x =>
                                        x.ManeuverOrder > -1)] =
                                new PerigeeHeightChangingManeuver(order, perigeeHeightManeuver.ManeuverHoldDuration.TotalSeconds,
                                    perigeeHeightManeuver.MinimumEpoch.SecondsFromJ2000TDB(),
                                    perigeeHeightManeuver.TargetPerigeeHeight);
                            //Add engines
                            for (int k = 0; k < perigeeHeightManeuver.Engines.Count; k++)
                            {
                                scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers[idx].Engines[k] =
                                    perigeeHeightManeuver.Engines.ElementAt(k).SerialNumber;
                            }

                            break;
                        }
                        case PhasingManeuver phasingManeuver:
                        {
                            StateVector target = _mapper.Map<StateVector>(phasingManeuver.TargetOrbit.ToStateVector());
                            int idx = scenarioDto.Spacecraft.PhasingManeuver.Count(x => x.ManeuverOrder > -1);
                            scenarioDto.Spacecraft.PhasingManeuver[
                                    scenarioDto.Spacecraft.PhasingManeuver.Count(x => x.ManeuverOrder > -1)] =
                                new DTO.PhasingManeuver(
                                    order,
                                    phasingManeuver.ManeuverHoldDuration.TotalSeconds, phasingManeuver.MinimumEpoch.SecondsFromJ2000TDB(),
                                    (int)phasingManeuver.RevolutionNumber, target);
                            //Add engines
                            for (int k = 0; k < phasingManeuver.Engines.Count; k++)
                            {
                                scenarioDto.Spacecraft.PhasingManeuver[idx].Engines[k] =
                                    phasingManeuver.Engines.ElementAt(k).SerialNumber;
                            }

                            break;
                        }
                        case InstrumentPointingToAttitude instManeuver:
                        {
                            int idx = scenarioDto.Spacecraft.PointingToAttitudes.Count(x => x.ManeuverOrder > -1);
                            scenarioDto.Spacecraft.PointingToAttitudes[
                                    scenarioDto.Spacecraft.PointingToAttitudes.Count(x => x.ManeuverOrder > -1)] =
                                new DTO.InstrumentPointingToAttitude(order, instManeuver.Instrument.NaifId,
                                    instManeuver.Target.NaifId,
                                    instManeuver.ManeuverHoldDuration.TotalSeconds,
                                    instManeuver.MinimumEpoch.SecondsFromJ2000TDB());
                            //Add engines
                            for (int k = 0; k < instManeuver.Engines.Count; k++)
                            {
                                scenarioDto.Spacecraft.PointingToAttitudes[idx].Engines[k] =
                                    instManeuver.Engines.ElementAt(k).SerialNumber;
                            }

                            break;
                        }
                        case ProgradeAttitude:
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

                            break;
                        }
                        case RetrogradeAttitude:
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

                            break;
                        }
                        case NadirAttitude:
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

                            break;
                        }
                        case ZenithAttitude:
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

                            break;
                        }
                    }

                    maneuver = maneuver.NextManeuver;
                    order++;
                }


                PropagateSpacecraftProxy(ref scenarioDto);

                if (scenarioDto.HasError())
                {
                    throw new InvalidOperationException($"Scenario can't be executed : {scenarioDto.Error}");
                }
                
                API.Instance.LoadKernels(siteDirectoryInfo);
                API.Instance.LoadKernels(spacecraftDirectoryInfo);

                foreach (var maneuverResult in
                         scenarioDto.Spacecraft.CombinedManeuvers.Where(x => x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.AttitudeWindow);
                    mnv.ThrustWindow = _mapper.Map<Time.Window>(maneuverResult.ThrustWindow);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.ManeuverWindow);
                    mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                    mnv.FuelBurned = maneuverResult.FuelBurned;
                }

                foreach (var maneuverResult in scenarioDto.Spacecraft.PhasingManeuver.Where(x => x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.AttitudeWindow);
                    mnv.ThrustWindow = _mapper.Map<Time.Window>(maneuverResult.ThrustWindow);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.ManeuverWindow);
                    mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                    mnv.FuelBurned = maneuverResult.FuelBurned;
                }

                foreach (var maneuverResult in scenarioDto.Spacecraft.ApsidalAlignmentManeuvers.Where(x =>
                             x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.AttitudeWindow);
                    mnv.ThrustWindow = _mapper.Map<Time.Window>(maneuverResult.ThrustWindow);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.ManeuverWindow);
                    mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                    mnv.FuelBurned = maneuverResult.FuelBurned;
                }

                foreach (var maneuverResult in scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers.Where(x =>
                             x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.AttitudeWindow);
                    mnv.ThrustWindow = _mapper.Map<Time.Window>(maneuverResult.ThrustWindow);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.ManeuverWindow);
                    mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                    mnv.FuelBurned = maneuverResult.FuelBurned;
                }

                foreach (var maneuverResult in scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers.Where(x =>
                             x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.AttitudeWindow);
                    mnv.ThrustWindow = _mapper.Map<Time.Window>(maneuverResult.ThrustWindow);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.ManeuverWindow);
                    mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                    mnv.FuelBurned = maneuverResult.FuelBurned;
                }

                foreach (var maneuverResult in scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers.Where(x =>
                             x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as ImpulseManeuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.AttitudeWindow);
                    mnv.ThrustWindow = _mapper.Map<Time.Window>(maneuverResult.ThrustWindow);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.ManeuverWindow);
                    mnv.DeltaV = _mapper.Map<Vector3>(maneuverResult.DeltaV);
                    mnv.FuelBurned = maneuverResult.FuelBurned;
                }

                foreach (var maneuverResult in scenarioDto.Spacecraft.NadirAttitudes.Where(x =>
                             x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as Maneuver.Maneuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.Window);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.Window);
                }

                foreach (var maneuverResult in scenarioDto.Spacecraft.ZenithAttitudes.Where(x =>
                             x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as Maneuver.Maneuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.Window);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.Window);
                }

                foreach (var maneuverResult in scenarioDto.Spacecraft.ProgradeAttitudes.Where(x =>
                             x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as Maneuver.Maneuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.Window);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.Window);
                }

                foreach (var maneuverResult in scenarioDto.Spacecraft.RetrogradeAttitudes.Where(x =>
                             x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as Maneuver.Maneuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.Window);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.Window);
                }

                foreach (var maneuverResult in scenarioDto.Spacecraft.PointingToAttitudes.Where(x =>
                             x.ManeuverOrder > -1))
                {
                    var mnv = spacecraft.GetManeuvers()[maneuverResult.ManeuverOrder] as Maneuver.Maneuver;
                    mnv.AttitudeWindow = _mapper.Map<Time.Window>(maneuverResult.Window);
                    mnv.ManeuverWindow = _mapper.Map<Time.Window>(maneuverResult.Window);
                }
            }
        }
    }


    /// <summary>
    ///     Load kernel at given path
    /// </summary>
    /// <param name="path">Path where kernels are located. This could be a file path or a directory path</param>
    public void LoadKernels(FileSystemInfo path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        lock (lockObject)
        {
            if (path.Exists)
            {
                if (!LoadKernelsProxy(path.FullName))
                {
                    throw new InvalidOperationException($"Kernel {path.FullName} can't be loaded. You can have more details on standard output");
                }
            }
        }
    }

    /// <summary>
    ///     Unload kernel at given path
    /// </summary>
    /// <param name="path">Path where kernels are located. This could be a file path or a directory path</param>
    public void UnloadKernels(FileSystemInfo path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        lock (lockObject)
        {
            if (path.Exists)
            {
                if (!UnloadKernelsProxy(path.FullName))
                {
                    throw new InvalidOperationException($"Kernel {path.FullName} can't be unloaded. You can have more details on standard output");
                }
            }
        }
    }

    /// <summary>
    ///     Find launch windows
    /// </summary>
    /// <param name="launch"></param>
    /// <param name="window"></param>
    /// <param name="outputDirectory"></param>
    public IEnumerable<LaunchWindow> FindLaunchWindows(Maneuver.Launch launch,
        in Time.Window window, DirectoryInfo outputDirectory)
    {
        if (launch == null) throw new ArgumentNullException(nameof(launch));
        lock (lockObject)
        {
            //Convert data
            Launch launchDto = _mapper.Map<Launch>(launch);
            launchDto.Window = _mapper.Map<Time.Window, Window>(window);
            launchDto.LaunchSite.DirectoryPath = outputDirectory.CreateSubdirectory("Sites").FullName;
            launchDto.RecoverySite.DirectoryPath = outputDirectory.CreateSubdirectory("Sites").FullName;

            //Execute request
            LaunchProxy(ref launchDto);

            if (launchDto.HasError())
            {
                throw new InvalidOperationException($"Operation failed to find launch windows : {launchDto.Error}");
            }

            //Filter result
            var windows = launchDto.Windows.Where(x => x.Start != 0 && x.End != 0).ToArray();

            //Build result 
            List<LaunchWindow> launchWindows = new List<LaunchWindow>();

            for (int i = 0; i < windows.Length; i++)
            {
                launchWindows.Add(new LaunchWindow(_mapper.Map<Window, Time.Window>(windows[i]),
                    launchDto.InertialInsertionVelocity[i], launchDto.NonInertialInsertionVelocity[i],
                    launchDto.InertialAzimuth[i], launchDto.NonInertialAzimuth[i]));
            }

            return launchWindows;
        }
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
    public IEnumerable<Time.Window> FindWindowsOnDistanceConstraint(Time.Window searchWindow, INaifObject observer,
        INaifObject target, RelationnalOperator relationalOperator, double value, Aberration aberration,
        TimeSpan stepSize)
    {
        if (observer == null) throw new ArgumentNullException(nameof(observer));
        if (target == null) throw new ArgumentNullException(nameof(target));
        lock (lockObject)
        {
            return FindWindowsOnDistanceConstraint(searchWindow, observer.NaifId, target.NaifId, relationalOperator, value, aberration, stepSize);
        }
    }
    
    public IEnumerable<Time.Window> FindWindowsOnDistanceConstraint(Time.Window searchWindow, int observerId,
        int targetId, RelationnalOperator relationalOperator, double value, Aberration aberration,
        TimeSpan stepSize)
    {
        lock (lockObject)
        {
            var windows = new Window[1000];
            for (var i = 0; i < 1000; i++)
            {
                windows[i] = new Window(double.NaN, double.NaN);
            }

            FindWindowsOnDistanceConstraintProxy(_mapper.Map<Window>(searchWindow), observerId, targetId, relationalOperator.GetDescription(), value,
                aberration.GetDescription(), stepSize.TotalSeconds, windows);
            return _mapper.Map<Time.Window[]>(windows.Where(x => !double.IsNaN(x.Start)));
        }
    }

    /// <summary>
    ///     Find time windows based on occultation constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="target"></param>
    /// <param name="targetShape"></param>
    /// <param name="frontBody"></param>
    /// <param name="frontShape"></param>
    /// <param name="occultationType"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    public IEnumerable<Time.Window> FindWindowsOnOccultationConstraint(Time.Window searchWindow, INaifObject observer,
        INaifObject target, ShapeType targetShape, INaifObject frontBody, ShapeType frontShape,
        OccultationType occultationType,
        Aberration aberration, TimeSpan stepSize)
    {
        if (observer == null) throw new ArgumentNullException(nameof(observer));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (frontBody == null) throw new ArgumentNullException(nameof(frontBody));
        lock (lockObject)
        {
            string frontFrame = frontShape == ShapeType.Ellipsoid
                ? (frontBody as Body.CelestialBody)?.Frame.Name
                : string.Empty;
            string targetFrame = targetShape == ShapeType.Ellipsoid
                ? (target as Body.CelestialBody)?.Frame.Name
                : String.Empty;
            var windows = new Window[1000];
            for (var i = 0; i < 1000; i++)
            {
                windows[i] = new Window(double.NaN, double.NaN);
            }

            FindWindowsOnOccultationConstraintProxy(_mapper.Map<Window>(searchWindow), observer.NaifId, target.NaifId,
                targetFrame, targetShape.GetDescription(),
                frontBody.NaifId, frontFrame, frontShape.GetDescription(), occultationType.GetDescription(),
                aberration.GetDescription(), stepSize.TotalSeconds, windows);
            return _mapper.Map<Time.Window[]>(windows.Where(x => !double.IsNaN(x.Start)));
        }
    }

    /// <summary>
    ///     Find time windows based on coordinate constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="target"></param>
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
    public IEnumerable<Time.Window> FindWindowsOnCoordinateConstraint(Time.Window searchWindow, INaifObject observer,
        INaifObject target, Frame frame, CoordinateSystem coordinateSystem, Coordinate coordinate,
        RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration,
        TimeSpan stepSize)
    {
        if (observer == null) throw new ArgumentNullException(nameof(observer));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        lock (lockObject)
        {
            var windows = new Window[1000];
            for (var i = 0; i < 1000; i++)
            {
                windows[i] = new Window(double.NaN, double.NaN);
            }

            FindWindowsOnCoordinateConstraintProxy(_mapper.Map<Window>(searchWindow), observer.NaifId, target.NaifId,
                frame.Name, coordinateSystem.GetDescription(),
                coordinate.GetDescription(), relationalOperator.GetDescription(), value, adjustValue,
                aberration.GetDescription(), stepSize.TotalSeconds, windows);
            return _mapper.Map<Time.Window[]>(windows.Where(x => !double.IsNaN(x.Start)));
        }
    }
    
    /// <summary>
    /// Find time windows based on coordinate constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="targetId"></param>
    /// <param name="frame"></param>
    /// <param name="coordinateSystem"></param>
    /// <param name="coordinate"></param>
    /// <param name="relationalOperator"></param>
    /// <param name="value"></param>
    /// <param name="adjustValue"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public IEnumerable<Time.Window> FindWindowsOnCoordinateConstraint(Time.Window searchWindow, int observerId,
        int targetId, Frame frame, CoordinateSystem coordinateSystem, Coordinate coordinate,
        RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration,
        TimeSpan stepSize)
    {
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        lock (lockObject)
        {
            var windows = new Window[1000];
            for (var i = 0; i < 1000; i++)
            {
                windows[i] = new Window(double.NaN, double.NaN);
            }

            FindWindowsOnCoordinateConstraintProxy(_mapper.Map<Window>(searchWindow), observerId, targetId,
                frame.Name, coordinateSystem.GetDescription(),
                coordinate.GetDescription(), relationalOperator.GetDescription(), value, adjustValue,
                aberration.GetDescription(), stepSize.TotalSeconds, windows);
            return _mapper.Map<Time.Window[]>(windows.Where(x => !double.IsNaN(x.Start)));
        }
    }

    /// <summary>
    ///     Find time windows based on illumination constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="illuminationSource"></param>
    /// <param name="observer"></param>
    /// <param name="targetBody"></param>
    /// <param name="fixedFrame"></param>
    /// <param name="planetodetic"></param>
    /// <param name="illuminationType"></param>
    /// <param name="relationalOperator"></param>
    /// <param name="value"></param>
    /// <param name="adjustValue"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public IEnumerable<Time.Window> FindWindowsOnIlluminationConstraint(Time.Window searchWindow, INaifObject observer,
        INaifObject targetBody, Frame fixedFrame,
        Coordinates.Planetodetic planetodetic, IlluminationAngle illuminationType, RelationnalOperator relationalOperator,
        double value, double adjustValue, Aberration aberration, TimeSpan stepSize, INaifObject illuminationSource,
        string method = "Ellipsoid")
    {
        if (observer == null) throw new ArgumentNullException(nameof(observer));
        if (targetBody == null) throw new ArgumentNullException(nameof(targetBody));
        if (fixedFrame == null) throw new ArgumentNullException(nameof(fixedFrame));
        if (illuminationSource == null) throw new ArgumentNullException(nameof(illuminationSource));
        if (method == null) throw new ArgumentNullException(nameof(method));
        lock (lockObject)
        {
            var windows = new Window[1000];
            for (var i = 0; i < 1000; i++)
            {
                windows[i] = new Window(double.NaN, double.NaN);
            }

            FindWindowsOnIlluminationConstraintProxy(_mapper.Map<Window>(searchWindow), observer.NaifId,
                illuminationSource.Name, targetBody.NaifId, fixedFrame.Name,
                _mapper.Map<Planetodetic>(planetodetic),
                illuminationType.GetDescription(), relationalOperator.GetDescription(), value, adjustValue,
                aberration.GetDescription(), stepSize.TotalSeconds, method, windows);
            return _mapper.Map<Time.Window[]>(windows.Where(x => !double.IsNaN(x.Start)));
        }
    }

    /// <summary>
    ///     Find time window when a target is in instrument's field of view
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observer"></param>
    /// <param name="instrument"></param>
    /// <param name="target"></param>
    /// <param name="targetFrame"></param>
    /// <param name="targetShape"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public IEnumerable<Time.Window> FindWindowsInFieldOfViewConstraint(Time.Window searchWindow, Spacecraft observer,
        Instrument instrument, INaifObject target, Frame targetFrame, ShapeType targetShape, Aberration aberration,
        TimeSpan stepSize)
    {
        if (observer == null) throw new ArgumentNullException(nameof(observer));
        if (instrument == null) throw new ArgumentNullException(nameof(instrument));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (targetFrame == null) throw new ArgumentNullException(nameof(targetFrame));
        lock (lockObject)
        {
            var windows = new Window[1000];
            for (var i = 0; i < 1000; i++)
            {
                windows[i] = new Window(double.NaN, double.NaN);
            }

            var searchWindowDto = _mapper.Map<Window>(searchWindow);

            FindWindowsInFieldOfViewConstraintProxy(searchWindowDto, observer.NaifId, instrument.NaifId, target.NaifId,
                targetFrame.Name,
                targetShape.GetDescription(), aberration.GetDescription(), stepSize.TotalSeconds, windows);

            return _mapper.Map<IEnumerable<Time.Window>>(windows.Where(x => !double.IsNaN(x.Start)).ToArray());
        }
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
    public IEnumerable<OrbitalParameters.OrbitalParameters> ReadEphemeris(Time.Window searchWindow,
        ILocalizable observer, ILocalizable target, Frame frame,
        Aberration aberration, TimeSpan stepSize)
    {
        if (observer == null) throw new ArgumentNullException(nameof(observer));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        lock (lockObject)
        {
            const int messageSize = 10000;
            List<OrbitalParameters.OrbitalParameters> orbitalParameters = new List<OrbitalParameters.OrbitalParameters>();
            int occurences = (int)(searchWindow.Length / stepSize / messageSize);


            for (int i = 0; i <= occurences; i++)
            {
                var start = searchWindow.StartDate + i * messageSize * stepSize;
                var end = start + messageSize * stepSize > searchWindow.EndDate ? searchWindow.EndDate : (start + messageSize * stepSize) - stepSize;
                var window = new Time.Window(start, end);
                var stateVectors = new StateVector[messageSize];
                ReadEphemerisProxy(_mapper.Map<Window>(window), observer.NaifId, target.NaifId, frame.Name,
                    aberration.GetDescription(), stepSize.TotalSeconds,
                    stateVectors);
                orbitalParameters.AddRange(stateVectors.Where(x => !string.IsNullOrEmpty(x.Frame)).Select(x =>
                    new OrbitalParameters.StateVector(_mapper.Map<Vector3>(x.Position),
                        _mapper.Map<Vector3>(x.Velocity), observer, DateTimeExtension.CreateTDB(x.Epoch), frame)));
            }

            return orbitalParameters;
        }
    }
    
    /// <summary>
    /// Return state vector at given epoch
    /// </summary>
    /// <param name="epoch"></param>
    /// <param name="observer"></param>
    /// <param name="target"></param>
    /// <param name="frame"></param>
    /// <param name="aberration"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public OrbitalParameters.OrbitalParameters ReadEphemeris(DateTime epoch, ILocalizable observer,
        ILocalizable target, Frame frame, Aberration aberration)
    {
        if (observer == null) throw new ArgumentNullException(nameof(observer));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        lock (lockObject)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            var stateVector = ReadEphemerisAtGivenEpochProxy(epoch.SecondsFromJ2000TDB(), observer.NaifId,
                target.NaifId, frame.Name, aberration.GetDescription());
            return new OrbitalParameters.StateVector(_mapper.Map<Vector3>(stateVector.Position),
                _mapper.Map<Vector3>(stateVector.Velocity), observer,
                DateTimeExtension.CreateTDB(stateVector.Epoch), frame);
        }
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
    public IEnumerable<OrbitalParameters.StateOrientation> ReadOrientation(Time.Window searchWindow,
        Spacecraft spacecraft, TimeSpan tolerance,
        Frame referenceFrame, TimeSpan stepSize)
    {
        if (spacecraft == null) throw new ArgumentNullException(nameof(spacecraft));
        if (referenceFrame == null) throw new ArgumentNullException(nameof(referenceFrame));
        lock (lockObject)
        {
            var stateOrientations = new StateOrientation[10000];
            ReadOrientationProxy(_mapper.Map<Window>(searchWindow), spacecraft.NaifId, tolerance.TotalSeconds,
                referenceFrame.Name, stepSize.TotalSeconds,
                stateOrientations);
            return stateOrientations.Select(x => new OrbitalParameters.StateOrientation(
                _mapper.Map<Quaternion>(x.Rotation), _mapper.Map<Vector3>(x.AngularVelocity),
                DateTimeExtension.CreateTDB(x.Epoch), referenceFrame));
        }
    }

    /// <summary>
    ///     Write ephemeris file
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="naifObject"></param>
    /// <param name="stateVectors"></param>
    /// <returns></returns>
    public bool WriteEphemeris(FileInfo filePath, INaifObject naifObject,
        IEnumerable<OrbitalParameters.StateVector> stateVectors)
    {
        if (naifObject == null) throw new ArgumentNullException(nameof(naifObject));
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        if (stateVectors == null) throw new ArgumentNullException(nameof(stateVectors));
        lock (lockObject)
        {
            var enumerable = stateVectors as OrbitalParameters.StateVector[] ?? stateVectors.ToArray();
            if (!enumerable.Any())
                throw new ArgumentException("Value cannot be an empty collection.", nameof(stateVectors));
            bool res = WriteEphemerisProxy(filePath.FullName, naifObject.NaifId,
                _mapper.Map<StateVector[]>(stateVectors),
                (uint)enumerable.Length);
            if (res == false)
            {
                throw new InvalidOperationException(
                    "An error occurred while writing ephemeris. You can have more details on standard output.");
            }

            return true;
        }
    }

    /// <summary>
    ///     Get celestial celestialItem information like radius, GM, name, associated frame, ...
    /// </summary>
    /// <param name="naifId"></param>
    /// <returns></returns>
    public CelestialBody GetCelestialBodyInfo(int naifId)
    {
        lock (lockObject)
        {
            var res = GetCelestialBodyInfoProxy(naifId);
            if (res.HasError())
            {
                throw new InvalidOperationException(
                    $"An error occured while reading celestial celestialItem information : {res.Error}");
            }

            return res;
        }
    }

    /// <summary>
    /// Transform a frame to another
    /// </summary>
    /// <param name="fromFrame"></param>
    /// <param name="toFrame"></param>
    /// <param name="epoch"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public OrbitalParameters.StateOrientation TransformFrame(Frame fromFrame, Frame toFrame, DateTime epoch)
    {
        lock (lockObject)
        {
            if (fromFrame == null) throw new ArgumentNullException(nameof(fromFrame));
            if (toFrame == null) throw new ArgumentNullException(nameof(toFrame));
            var res = TransformFrameProxy(fromFrame.Name, toFrame.Name, epoch.ToTDB().SecondsFromJ2000TDB());
            if (res.HasError())
            {
                throw new InvalidOperationException($"An error occured during frame transformation : {res.Error}");
            }

            return new OrbitalParameters.StateOrientation(
                new Quaternion(res.Rotation.W, res.Rotation.X, res.Rotation.Y, res.Rotation.Z),
                new Vector3(res.AngularVelocity.X, res.AngularVelocity.Y, res.AngularVelocity.Z), epoch, fromFrame);
        }
    }

    /// <summary>
    /// Convert TLE to state vector at given epoch
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <param name="line3"></param>
    /// <param name="epoch"></param>
    /// <returns></returns>
    public OrbitalParameters.StateVector ConvertTleToStateVector(string line1, string line2, string line3,
        DateTime epoch)
    {
        lock (lockObject)
        {
            var res = ConvertTLEToStateVectorProxy(line1, line2, line3, epoch.SecondsFromJ2000TDB());
            return new OrbitalParameters.StateVector(_mapper.Map<Vector3>(res.Position),
                _mapper.Map<Vector3>(res.Velocity),
                new Body.CelestialBody(PlanetsAndMoons.EARTH, Frame.ECLIPTIC_J2000, epoch), epoch,
                new Frame(res.Frame));
        }
    }

    /// <summary>
    /// Create TLE
    /// </summary>
    /// <param name="line1">CelestialItem identifier</param>
    /// <param name="line2">line 1</param>
    /// <param name="line3">line 2</param>
    /// <returns></returns>
    public TLE CreateTLE(string line1, string line2, string line3)
    {
        lock (lockObject)
        {
            var res = GetTLEElementsProxy(line1, line2, line3);
            if (res.HasError())
            {
                throw new InvalidOperationException($"An error occured during TLE creation : {res.Error}");
            }

            return new TLE(line1, line2, line3, res.BalisticCoefficient, res.DragTerm, res.SecondDerivativeOfMeanMotion,
                res.A, res.E, res.I, res.O, res.W, res.M,
                DateTimeExtension.CreateTDB(res.Epoch), Frame.ICRF);
        }
    }
}