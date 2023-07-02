# IO.SDK.Net
## Presentation
[![Continous integration](https://github.com/IO-Aerospace-software-engineering/SDK.Net/actions/workflows/ci.yml/badge.svg)](https://github.com/IO-Aerospace-software-engineering/SDK.Net/actions/workflows/ci.yml)
[![Continous deployment](https://github.com/IO-Aerospace-software-engineering/SDK.Net/actions/workflows/cd.yml/badge.svg)](https://github.com/IO-Aerospace-software-engineering/SDK.Net/actions/workflows/cd.yml)

IO.SDK.Net is the .Net connector to [IO.SDK native library](https://github.com/IO-Aerospace-software-engineering/SDK).

It allows .Net developers to call IO.SDK high level features :

* Load [JPL Spice kernels](https://naif.jpl.nasa.gov/naif/data.html)
* Compute and convert orbital parameters
  * State vector
  * Two lines elements
  * Equinoctial
  * Keplerian elements
* Compute and convert coordinates system
  * Equatorial
  * Horizontal
  * Geodetic
* Frame transformation
  * ICRF
  * Ecliptic
  * Body fixed frames
  * ITRF93 (High accuracy earth fixed frame)
* Execute spacecraft propagator to evaluate trajectory, maneuvers and fuel balance
* Impulse maneuvers :
  * Apogee height
  * Perigee height
  * Plane alignment
  * Combined maneuver
  * Apsidal alignment
  * Phasing
* Attitudes
  * Instrument pointing toward an object
  * Nadir
  * Zenith
  * Prograde
  * Retrograde
  * Zenith
* Configure spacecraft
  * Clock
  * Fuel tank
  * Engines
  * Instrument
* Surface site on any celestial body
* Evaluate launch opportunities
* Use or convert different time referential (TDB, UTC, Local)
* Get celestial body information based on Naif kernels
* Find time windows based on distance constraints from spacecraft, celestial body or ground site
* Find time windows based on occultation constraints from spacecraft, celestial body or ground site
* Find time windows based on coordinate constraints from spacecraft, celestial body or ground site
* Find time windows based on illumination constraints from ground site.
* Find time windows when an object is in instrument field of view.
* Manipulate kernel files
* Math tools
  * Vector
  * Matrix
  * Quaternion
  * Lagrange interpolation


## Installation
This package is hosted by Nuget [here](https://www.nuget.org/packages/IO.Astrodynamics/).
You can install it in your project with this command :
```
dotnet add package IO.Astrodynamics
```
## Quick start
```C#
//LET'S GO !
//In this example we'll get the moon state vector in ICRF frame relative to the earth without aberration

//Load required kernels for computation
API.Instance.LoadKernels(new DirectoryInfo("/home/spacer/Sources/SDK.Net/IO.Astrodynamics.Tests/Data/SolarSystem"));

//Create moon object
var moon = new CelestialBody(PlanetsAndMoons.MOON.NaifId);

//Get moon ephemeris
var ephemeris = moon.GetEphemeris(DateTimeExtension.J2000, moon.InitialOrbitalParameters.CenterOfMotion, Frame.ICRF, Aberration.None).ToStateVector();

//Display some informations
Console.WriteLine($"Position : {ephemeris.Position.ToString()}");
Console.WriteLine($"Velocity : {ephemeris.Velocity.ToString()}");

//You should have the following result : 
// Position : Vector3 { X = -291608384.6334355, Y = -266716833.39423338, Z = -76102487.09990202 }
// Velocity : Vector3 { X = 643.5313877190328, Y = -666.0876840916304, Z = -301.32570498227307 }

```

You can find more advanced examples [here](https://github.com/IO-Aerospace-software-engineering/Astrodynamics.Net/wiki/Examples)

## Documentation
For more information you can read the [wiki](https://github.com/IO-Aerospace-software-engineering/Astrodynamics.Net/wiki)
