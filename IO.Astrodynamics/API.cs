// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
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
using ApsidalAlignmentManeuver = IO.Astrodynamics.DTO.ApsidalAlignmentManeuver;
using CombinedManeuver = IO.Astrodynamics.DTO.CombinedManeuver;
using Launch = IO.Astrodynamics.DTO.Launch;
using PhasingManeuver = IO.Astrodynamics.DTO.PhasingManeuver;
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
        if (_isResolverLoaded) return;
        NativeLibrary.SetDllImportResolver(typeof(API).Assembly, Resolver);
        _isResolverLoaded = true;
        _mapper = ProfilesConfiguration.Instance.Mapper;
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
    ///     Convert seconds from J2000 to formatted string
    /// </summary>
    /// <param name="secondsFromJ2000"></param>
    /// <returns></returns>
    public string TDBToString(double secondsFromJ2000)
    {
        return TDBToStringProxy(secondsFromJ2000);
    }

    /// <summary>
    ///     Convert seconds from J2000 to formatted string
    /// </summary>
    /// <param name="secondsFromJ2000"></param>
    /// <returns></returns>
    public string UTCToString(double secondsFromJ2000)
    {
        return UTCToStringProxy(secondsFromJ2000);
    }

    /// <summary>
    ///     Execute the scenario
    /// </summary>
    /// <param name="scenario"></param>
    public void ExecuteScenario(Models.Mission.Scenario scenario)
    {
        for (int i = 0; i < scenario.Bodies.OfType<Models.Mission.SpacecraftScenario>().Count(); i++)
        {
            Scenario scenarioDto = new Scenario(scenario.Name, new Window(scenario.Window.StartDate.SecondsFromJ2000(), scenario.Window.EndDate.SecondsFromJ2000()));


            for (int j = 0; j < scenario.Bodies.OfType<Models.Mission.CelestialBodyScenario>().Count(); j++)
            {
                scenarioDto.CelestialBodiesId[i] = scenario.Bodies.ElementAt(i).PhysicalBody.NaifId;
            }

            var spacecraft = scenario.Bodies.ElementAt(i) as SpacecraftScenario;
            //Define parking orbit
            var sv = spacecraft.InitialOrbitalParameters.ToStateVector();
            StateVector parkingOrbit = _mapper.Map<StateVector>(spacecraft.InitialOrbitalParameters.ToStateVector());

            //Create and configure spacecraft
            scenarioDto.Spacecraft = new Spacecraft(spacecraft.PhysicalBody.NaifId, spacecraft.PhysicalBody.Name, spacecraft.PhysicalBody.DryOperatingMass,
                spacecraft.PhysicalBody.MaximumOperatingMass, parkingOrbit, spacecraft.SpacecraftDirectory.FullName);
            for (int j = 0; j < spacecraft.FuelTanks.Count; j++)
            {
                var fuelTank = spacecraft.FuelTanks.ElementAt(j);
                scenarioDto.Spacecraft.FuelTanks[j] = new FuelTank(j + 1, capacity: fuelTank.FuelTank.Capacity, quantity: fuelTank.Quantity, serialNumber: fuelTank.SerialNumber);
            }

            for (int j = 0; j < spacecraft.Engines.Count; j++)
            {
                var engine = spacecraft.Engines.ElementAt(j);
                scenarioDto.Spacecraft.Engines[0] = new EngineDTO(id: j + 1, name: engine.Engine.Name, fuelFlow: engine.Engine.FuelFlow, serialNumber: engine.SerialNumber,
                    fuelTankSerialNumber: engine.FuelTank.SerialNumber, isp: engine.Engine.ISP);
            }

            for (int j = 0; j < spacecraft.Payloads.Count; j++)
            {
                var payload = spacecraft.Payloads.ElementAt(j);
                scenarioDto.Spacecraft.Payloads[0] = new Payload(payload.SerialNumber, payload.Name, payload.Mass);
            }

            for (int j = 0; j < spacecraft.Intruments.Count; j++)
            {
                var instrument = spacecraft.Intruments.ElementAt(j);
                var orientation = instrument.Orientation.ToEuler();
                scenarioDto.Spacecraft.Instruments[0] = new Instrument(instrument.Instrument.NaifId, instrument.Instrument.Name, instrument.Instrument.Shape.GetDescription(),
                    _mapper.Map<Vector3D>(orientation), _mapper.Map<Vector3D>(Models.Body.Spacecraft.Instrument.Boresight),
                    _mapper.Map<Vector3D>(Models.Body.Spacecraft.Instrument.RefVector), instrument.Instrument.FieldOfView, instrument.Instrument.CrossAngle);
            }

            var maneuver = spacecraft.StandbyManeuver;
            int order = 0;
            while (maneuver != null)
            {
                StateVector target = _mapper.Map<StateVector>(maneuver.TargetOrbit.ToStateVector());
                if (maneuver is PlaneAlignmentManeuver)
                {
                    int idx = scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers[idx] = new OrbitalPlaneChangingManeuver(order, maneuver.ManeuverHoldDuration.TotalSeconds,
                        maneuver.MinimumEpoch.SecondsFromJ2000(), target);

                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.OrbitalPlaneChangingManeuvers[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is ApogeeHeightManeuver)
                {
                    int idx = scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers[scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers.Count(x => x.ManeuverOrder > -1)] =
                        new ApogeeHeightChangingManeuver(order, maneuver.ManeuverHoldDuration.TotalSeconds, maneuver.MinimumEpoch.SecondsFromJ2000(),
                            (maneuver as ApogeeHeightManeuver).TargetApogee);

                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.ApogeeHeightChangingManeuvers[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is IO.Astrodynamics.Models.Maneuver.ApsidalAlignmentManeuver)
                {
                    int idx = scenarioDto.Spacecraft.ApsidalAlignmentManeuvers.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.ApsidalAlignmentManeuvers[scenarioDto.Spacecraft.ApsidalAlignmentManeuvers.Count(x => x.ManeuverOrder > -1)] =
                        new ApsidalAlignmentManeuver(order, maneuver.ManeuverHoldDuration.TotalSeconds, maneuver.MinimumEpoch.SecondsFromJ2000(), target);

                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.ApsidalAlignmentManeuvers[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is IO.Astrodynamics.Models.Maneuver.CombinedManeuver)
                {
                    int idx = scenarioDto.Spacecraft.CombinedManeuvers.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.CombinedManeuvers[scenarioDto.Spacecraft.CombinedManeuvers.Count(x => x.ManeuverOrder > -1)] = new CombinedManeuver(order,
                        maneuver.ManeuverHoldDuration.TotalSeconds, maneuver.MinimumEpoch.SecondsFromJ2000(), maneuver.TargetOrbit.ApogeeVector().Magnitude(),
                        maneuver.TargetOrbit.Inclination());

                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.CombinedManeuvers[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is PerigeeHeightManeuver)
                {
                    int idx = scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers[scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers.Count(x => x.ManeuverOrder > -1)] =
                        new PerigeeHeightChangingManeuver(order, maneuver.ManeuverHoldDuration.TotalSeconds,
                            maneuver.MinimumEpoch.SecondsFromJ2000(), maneuver.TargetOrbit.PerigeeVector().Magnitude());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.PerigeeHeightChangingManeuvers[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is IO.Astrodynamics.Models.Maneuver.PhasingManeuver)
                {
                    int idx = scenarioDto.Spacecraft.PhasingManeuver.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.PhasingManeuver[scenarioDto.Spacecraft.PhasingManeuver.Count(x => x.ManeuverOrder > -1)] = new PhasingManeuver(order,
                        maneuver.ManeuverHoldDuration.TotalSeconds, maneuver.MinimumEpoch.SecondsFromJ2000(),
                        (int)(maneuver as IO.Astrodynamics.Models.Maneuver.PhasingManeuver).RevolutionNumber, target);
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.PhasingManeuver[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is Models.Maneuver.InstrumentPointingToAttitude)
                {
                    int idx = scenarioDto.Spacecraft.PointingToAttitudes.Count(x => x.ManeuverOrder > -1);
                    var instManeuver = maneuver as Models.Maneuver.InstrumentPointingToAttitude;
                    scenarioDto.Spacecraft.PointingToAttitudes[scenarioDto.Spacecraft.PointingToAttitudes.Count(x => x.ManeuverOrder > -1)] =
                        new DTO.InstrumentPointingToAttitude(order, instManeuver.Instrument.Instrument.NaifId, instManeuver.TargetId.NaifId,
                            instManeuver.ManeuverHoldDuration.TotalSeconds, instManeuver.MinimumEpoch.SecondsFromJ2000());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.PointingToAttitudes[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is Models.Maneuver.ProgradeAttitude)
                {
                    int idx = scenarioDto.Spacecraft.ProgradeAttitudes.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.ProgradeAttitudes[scenarioDto.Spacecraft.ProgradeAttitudes.Count(x => x.ManeuverOrder > -1)] =
                        new DTO.ProgradeAttitude(order, maneuver.ManeuverHoldDuration.TotalSeconds, maneuver.MinimumEpoch.SecondsFromJ2000());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.ProgradeAttitudes[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is Models.Maneuver.RetrogradeAttitude)
                {
                    int idx = scenarioDto.Spacecraft.RetrogradeAttitudes.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.RetrogradeAttitudes[scenarioDto.Spacecraft.RetrogradeAttitudes.Count(x => x.ManeuverOrder > -1)] =
                        new DTO.RetrogradeAttitude(order, maneuver.ManeuverHoldDuration.TotalSeconds, maneuver.MinimumEpoch.SecondsFromJ2000());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.RetrogradeAttitudes[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is Models.Maneuver.NadirAttitude)
                {
                    int idx = scenarioDto.Spacecraft.NadirAttitudes.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.NadirAttitudes[scenarioDto.Spacecraft.NadirAttitudes.Count(x => x.ManeuverOrder > -1)] =
                        new DTO.NadirAttitude(order, maneuver.ManeuverHoldDuration.TotalSeconds, maneuver.MinimumEpoch.SecondsFromJ2000());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.NadirAttitudes[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }
                else if (maneuver is Models.Maneuver.ZenithAttitude)
                {
                    int idx = scenarioDto.Spacecraft.ZenithAttitudes.Count(x => x.ManeuverOrder > -1);
                    scenarioDto.Spacecraft.ZenithAttitudes[scenarioDto.Spacecraft.ZenithAttitudes.Count(x => x.ManeuverOrder > -1)] =
                        new DTO.ZenithAttitude(order, maneuver.ManeuverHoldDuration.TotalSeconds, maneuver.MinimumEpoch.SecondsFromJ2000());
                    //Add engines
                    for (int k = 0; k < maneuver.Engines.Count; k++)
                    {
                        scenarioDto.Spacecraft.ZenithAttitudes[idx].Engines[k] = maneuver.Engines.ElementAt(k).SerialNumber;
                    }
                }

                maneuver = maneuver.NextManeuver;
                order++;
            }

            PropagateProxy(ref scenarioDto);
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
    public void FindLaunchWindows(IO.Astrodynamics.Models.Maneuver.Launch launch,in Models.Time.Window window)
    {
        Launch launchDto = _mapper.Map<Launch>(launch);
        launchDto.Window = _mapper.Map<Models.Time.Window, Window>(window);
        
        LaunchProxy(ref launchDto);
    }

    /// <summary>
    ///     Find time windows based on distance constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="targetId"></param>
    /// <param name="relationnalOperator"></param>
    /// <param name="value"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public Window[] FindWindowsOnDistanceConstraint(Window searchWindow, int observerId,
        int targetId, RelationnalOperator relationnalOperator, double value, Aberration aberration, TimeSpan stepSize)
    {
        var windows = new Window[1000];
        for (var i = 0; i < 1000; i++)
        {
            windows[i] = new Window(double.NaN, double.NaN);
        }

        FindWindowsOnDistanceConstraintProxy(searchWindow, observerId, targetId, relationnalOperator.GetDescription(),
            value,
            aberration.GetDescription(),
            stepSize.TotalSeconds, windows);
        return windows.Where(x => !double.IsNaN(x.Start)).ToArray();
    }

    /// <summary>
    ///     Find time windows based on occultation constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="targetId"></param>
    /// <param name="targetFrame"></param>
    /// <param name="targetShape"></param>
    /// <param name="frontBodyId"></param>
    /// <param name="frontFrame"></param>
    /// <param name="frontShape"></param>
    /// <param name="occultationType"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public Window[] FindWindowsOnOccultationConstraint(Window searchWindow, int observerId,
        int targetId, string targetFrame, ShapeType targetShape, int frontBodyId, string frontFrame,
        ShapeType frontShape,
        OccultationType occultationType, Aberration aberration, TimeSpan stepSize)
    {
        if (targetFrame == null) throw new ArgumentNullException(nameof(targetFrame));
        if (frontFrame == null) throw new ArgumentNullException(nameof(frontFrame));
        var windows = new Window[1000];
        for (var i = 0; i < 1000; i++)
        {
            windows[i] = new Window(double.NaN, double.NaN);
        }

        FindWindowsOnOccultationConstraintProxy(searchWindow, observerId, targetId, targetFrame,
            targetShape.GetDescription(),
            frontBodyId, frontFrame, frontShape.GetDescription(), occultationType.GetDescription(),
            aberration.GetDescription(), stepSize.TotalSeconds, windows);
        return windows.Where(x => !double.IsNaN(x.Start)).ToArray();
    }

    /// <summary>
    ///     Find time windows based on coordinate constraint
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
    public Window[] FindWindowsOnCoordinateConstraint(Window searchWindow, int observerId, int targetId,
        string frame, CoordinateSystem coordinateSystem, Coordinate coordinate,
        RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration,
        TimeSpan stepSize)
    {
        var windows = new Window[1000];
        for (var i = 0; i < 1000; i++)
        {
            windows[i] = new Window(double.NaN, double.NaN);
        }

        FindWindowsOnCoordinateConstraintProxy(searchWindow, observerId, targetId, frame,
            coordinateSystem.GetDescription(),
            coordinate.GetDescription(), relationalOperator.GetDescription(), value, adjustValue,
            aberration.GetDescription(), stepSize.TotalSeconds, windows);
        return windows.Where(x => !double.IsNaN(x.Start)).ToArray();
    }

    /// <summary>
    ///     Find time windows based on illumination constraint
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="illuminationSource"></param>
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
    public Window[] FindWindowsOnIlluminationConstraint(Window searchWindow, int observerId, int targetBody,
        string fixedFrame, Geodetic geodetic, IlluminationAngle illuminationType,
        RelationnalOperator relationalOperator, double value, double adjustValue,
        Aberration aberration, TimeSpan stepSize, string illuminationSource = "SUN", string method = "Ellipsoid")
    {
        if (fixedFrame == null) throw new ArgumentNullException(nameof(fixedFrame));
        if (illuminationSource == null) throw new ArgumentNullException(nameof(illuminationSource));
        if (method == null) throw new ArgumentNullException(nameof(method));
        var windows = new Window[1000];
        for (var i = 0; i < 1000; i++)
        {
            windows[i] = new Window(double.NaN, double.NaN);
        }

        FindWindowsOnIlluminationConstraintProxy(searchWindow, observerId, illuminationSource, targetBody, fixedFrame,
            geodetic, illuminationType.GetDescription(), relationalOperator.GetDescription(), value, adjustValue,
            aberration.GetDescription(), stepSize.TotalSeconds,
            method, windows);
        return windows.Where(x => !double.IsNaN(x.Start)).ToArray();
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
    public Window[] FindWindowsInFieldOfViewConstraint(Window searchWindow, int observerId,
        int instrumentId, int targetId, string targetFrame, ShapeType targetShape, Aberration aberration,
        TimeSpan stepSize)
    {
        if (targetFrame == null) throw new ArgumentNullException(nameof(targetFrame));
        var windows = new Window[1000];
        for (var i = 0; i < 1000; i++)
        {
            windows[i] = new Window(double.NaN, double.NaN);
        }

        FindWindowsInFieldOfViewConstraintProxy(searchWindow, observerId, observerId * 1000 - instrumentId, targetId,
            targetFrame, targetShape.GetDescription(), aberration.GetDescription(), stepSize.TotalSeconds, windows);
        return windows.Where(x => !double.IsNaN(x.Start)).ToArray();
    }

    /// <summary>
    ///     Read object ephemeris for a given period
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="observerId"></param>
    /// <param name="targetId"></param>
    /// <param name="frame"></param>
    /// <param name="aberration"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public StateVector[] ReadEphemeris(Window searchWindow, int observerId, int targetId, string frame,
        Aberration aberration, TimeSpan stepSize)
    {
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        var stateVectors = new StateVector[5000];
        ReadEphemerisProxy(searchWindow, observerId, targetId, frame, aberration.GetDescription(),
            stepSize.TotalSeconds,
            stateVectors);
        return stateVectors;
    }

    /// <summary>
    ///     Read spacecraft orientation for a given period
    /// </summary>
    /// <param name="searchWindow"></param>
    /// <param name="spacecraftId"></param>
    /// <param name="tolerance"></param>
    /// <param name="referenceFrame"></param>
    /// <param name="stepSize"></param>
    /// <returns></returns>
    public StateOrientation[] ReadOrientation(Window searchWindow, int spacecraftId, double tolerance,
        string referenceFrame, TimeSpan stepSize)
    {
        if (referenceFrame == null) throw new ArgumentNullException(nameof(referenceFrame));
        var stateOrientations = new StateOrientation[10000];
        ReadOrientationProxy(searchWindow, spacecraftId, tolerance, referenceFrame, stepSize.TotalSeconds,
            stateOrientations);
        return stateOrientations;
    }

    /// <summary>
    ///     Convert UTC to TDB seconds elapsed from J2000
    /// </summary>
    /// <param name="utc"></param>
    /// <returns></returns>
    public double ConvertUTCToTDB(double utc)
    {
        return ConvertUTCToTDBProxy(utc);
    }

    /// <summary>
    ///     Convert TDB to UTC seconds elapsed from J2000
    /// </summary>
    /// <param name="tdb"></param>
    /// <returns></returns>
    public double ConvertTDBToUTC(double tdb)
    {
        return ConvertTDBToUTCProxy(tdb);
    }

    /// <summary>
    ///     Write ephemeris file
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="objectId"></param>
    /// <param name="stateVectors"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public bool WriteEphemeris(FileInfo filePath, int objectId, StateVector[] stateVectors, uint size)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        if (stateVectors == null) throw new ArgumentNullException(nameof(stateVectors));
        if (stateVectors.Length == 0)
            throw new ArgumentException("Value cannot be an empty collection.", nameof(stateVectors));
        return WriteEphemerisProxy(filePath.FullName, objectId, stateVectors, size);
    }

    /// <summary>
    ///     Get celestial body information like radius, GM, name, associated frame, ...
    /// </summary>
    /// <param name="celestialBodyId"></param>
    /// <returns></returns>
    public CelestialBody GetCelestialBodyInfo(int celestialBodyId)
    {
        return GetCelestialBodyInfoProxy(celestialBodyId);
    }

    public Models.OrbitalParameters.StateOrientation TransformFrame(Frame fromFrame, Frame toFrame, DateTime epoch)
    {
        if (fromFrame == null) throw new ArgumentNullException(nameof(fromFrame));
        if (toFrame == null) throw new ArgumentNullException(nameof(toFrame));
        var res = TransformFrameProxy(fromFrame.Name, toFrame.Name, epoch.ToTDB().SecondsFromJ2000());
        return new Models.OrbitalParameters.StateOrientation(
            new IO.Astrodynamics.Models.Math.Quaternion(res.Rotation.W, res.Rotation.X, res.Rotation.Y, res.Rotation.Z),
            new IO.Astrodynamics.Models.Math.Vector3(res.AngularVelocity.X, res.AngularVelocity.Y, res.AngularVelocity.Z), epoch, fromFrame);
    }

    public StateVector ConvertToStateVector(EquinoctialElements equinoctialElements)
    {
        return ConvertEquinoctialElementsToStateVectorProxy(equinoctialElements);
    }

    public StateVector ConvertToStateVector(ConicElements conicElements)
    {
        return ConvertConicElementsToStateVectorProxy(conicElements);
    }

    public RaDec ConvertToEquatorialCoordinates(StateVector stateVector)
    {
        return ConvertStateVectorToEquatorialCoordinatesProxy(stateVector);
    }

    public RaDec ConvertToEquatorialCoordinates(ConicElements conicElements)
    {
        var sv = ConvertToStateVector(conicElements);
        return ConvertStateVectorToEquatorialCoordinatesProxy(sv);
    }

    public RaDec ConvertToEquatorialCoordinates(EquinoctialElements equinoctialElements)
    {
        var sv = ConvertToStateVector(equinoctialElements);
        return ConvertStateVectorToEquatorialCoordinatesProxy(sv);
    }
}