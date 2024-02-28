using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IO.Astrodynamics.Surface;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Mission
{
    public class Scenario : IEquatable<Scenario>
    {
        public string Name { get; }
        public Window Window { get; }
        public Mission Mission { get; }
        private readonly HashSet<Body.CelestialBody> _additionalCelestialBodies = new();
        public IReadOnlyCollection<Body.CelestialBody> AdditionalCelstialBodies => _additionalCelestialBodies;

        private readonly HashSet<Body.Spacecraft.Spacecraft> _spacecrafts = new();
        public IReadOnlyCollection<Body.Spacecraft.Spacecraft> Spacecrafts => _spacecrafts;
        private readonly HashSet<Site> _sites = new();
        public IReadOnlyCollection<Site> Sites => _sites;
        public DirectoryInfo RootDirectory { get; private set; }
        public DirectoryInfo SpacecraftDirectory { get; private set; }
        public DirectoryInfo SiteDirectory { get; private set; }

        public bool IsSimulated => RootDirectory?.Exists == true && (SpacecraftDirectory?.Exists == true || SiteDirectory?.Exists == true);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Scenario name</param>
        /// <param name="mission">Mission</param>
        /// <param name="window">Time window for propagation</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public Scenario(string name, Mission mission, in Window window)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            Name = name;
            Mission = mission ?? throw new ArgumentNullException(nameof(mission));
            Window = window;
        }

        /// <summary>
        /// By default the framework use recursively centers of motions up to the root (ex. moon->earth->sun) but with this method you can add another celestial celestialItem to improve propagation accuracy if needed
        /// </summary>
        /// <param name="celestialBody"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddAdditionalCelestialBody(Body.CelestialBody celestialBody)
        {
            if (celestialBody == null) throw new ArgumentNullException(nameof(celestialBody));
            _additionalCelestialBodies.Add(celestialBody);
        }

        /// <summary>
        /// Add spacecraft to scenario
        /// </summary>
        /// <param name="spacecraft"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddSpacecraft(Body.Spacecraft.Spacecraft spacecraft)
        {
            if (spacecraft == null) throw new ArgumentNullException(nameof(spacecraft));
            _spacecrafts.Add(spacecraft);
        }

        /// <summary>
        /// Add site to scenario
        /// </summary>
        /// <param name="site"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddSite(Site site)
        {
            if (site == null) throw new ArgumentNullException(nameof(site));
            _sites.Add(site);
        }

        /// <summary>
        /// Propagate this scenario
        /// </summary>
        /// <param name="outputDirectory">Output folder used to write files</param>
        /// <param name="includeAtmosphericDrag"></param>
        /// <param name="includeSolarRadiationPressure"></param>
        /// <param name="propagatorStepSize"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ScenarioSummary> SimulateAsync(DirectoryInfo outputDirectory, bool includeAtmosphericDrag, bool includeSolarRadiationPressure, TimeSpan propagatorStepSize)
        {
            InitializeDirectories(outputDirectory);

            try
            {
                foreach (var spacecraft in _spacecrafts)
                {
                    var spacecraftDirectory = SpacecraftDirectory.CreateSubdirectory(spacecraft.Name);
                    Propagator.SpacecraftPropagator spacecraftPropagator = new Propagator.SpacecraftPropagator(Window, spacecraft, _additionalCelestialBodies,
                        includeAtmosphericDrag, includeSolarRadiationPressure, propagatorStepSize);
                    var res = spacecraftPropagator.Propagate();
                    //Write frame
                    await spacecraft.WriteFrameAsync(new FileInfo(Path.Combine(spacecraftDirectory.CreateSubdirectory("Frames").FullName,
                        spacecraft.Name + ".tf")));

                    
                    //write instrument frame and kernel 
                    var instrumentDirectory = spacecraftDirectory.CreateSubdirectory("Instruments");
                    foreach (var instrument in spacecraft.Instruments)
                    {
                        await instrument.WriteFrameAsync(new FileInfo(Path.Combine(instrumentDirectory.FullName, instrument.Name + ".tf")));
                        await instrument.WriteKernelAsync(new FileInfo(Path.Combine(instrumentDirectory.FullName, instrument.Name + ".ti")));
                    }

                    //Write clock
                    var clockFile = new FileInfo(Path.Combine(spacecraftDirectory.CreateSubdirectory("Clocks").FullName, spacecraft.Name + ".tsc"));
                    await spacecraft.Clock.WriteAsync(clockFile);
                    
                    //Write Ephemeris
                    API.Instance.WriteEphemeris(new FileInfo(Path.Combine(spacecraftDirectory.CreateSubdirectory("Ephemeris").FullName, spacecraft.Name + ".spk")), spacecraft,
                        res.stateVectors);
                    
                    //Clock is loaded because is needed by orientation writer
                    API.Instance.LoadKernels(clockFile);
                    //Write Orientation
                    API.Instance.WriteOrientation(new FileInfo(Path.Combine(spacecraftDirectory.CreateSubdirectory("Orientation").FullName, spacecraft.Name + ".ck")), spacecraft,
                        res.stateOrientations);
                }
            }
            finally
            {
                API.Instance.UnloadKernels(SiteDirectory);
                API.Instance.UnloadKernels(SpacecraftDirectory);
            }

            ScenarioSummary scenarioSummary = new ScenarioSummary(this.Window, SiteDirectory, SpacecraftDirectory);
            foreach (var spacecraft in _spacecrafts)
            {
                scenarioSummary.AddSpacecraftSummary(spacecraft.GetSummary());
            }

            return scenarioSummary;
        }

        private void InitializeDirectories(DirectoryInfo outputDirectory)
        {
            RootDirectory = null;
            SpacecraftDirectory = null;
            SiteDirectory = null;

            if (_spacecrafts.Count == 0 && _sites.Count == 0)
            {
                throw new InvalidOperationException("There is nothing to simulate");
            }

            RootDirectory = outputDirectory.CreateSubdirectory(Mission.Name).CreateSubdirectory(this.Name);
            SpacecraftDirectory = RootDirectory.CreateSubdirectory("Spacecrafts");
            SiteDirectory = RootDirectory.CreateSubdirectory("Sites");
        }

        public bool Equals(Scenario other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Mission == other.Mission;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Scenario)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(Scenario left, Scenario right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Scenario left, Scenario right)
        {
            return !Equals(left, right);
        }
    }
}