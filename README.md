# IO.SDK.Net
## Presentation
[![Continous integration](https://github.com/IO-Aerospace-software-engineering/SDK.Net/actions/workflows/ci.yml/badge.svg)](https://github.com/IO-Aerospace-software-engineering/SDK.Net/actions/workflows/ci.yml)
[![Continous deployment](https://github.com/IO-Aerospace-software-engineering/SDK.Net/actions/workflows/cd.yml/badge.svg)](https://github.com/IO-Aerospace-software-engineering/SDK.Net/actions/workflows/cd.yml)

IO.SDK.Net is the .Net connector to [IO.SDK native library](https://github.com/IO-Aerospace-software-engineering/SDK).

It allows .Net developers to call IO.SDK high level features :

* Load [JPL Spice kernels](https://naif.jpl.nasa.gov/naif/data.html)
* Execute spacecraft propagator to evaluate maneuvers and fuel consumption
* Evaluate launch opportunities
* Convert elapsed seconds from J2000 to TDB or UTC string
* Find time windows based on distance constraints
* Find time windows based on occultation constraints
* Find time windows based on coordinate constraints
* Find time windows based on illumination constraints
* Find time windows when an object is in instrument field of view.

## Installation
This package is hosted by Nuget [here](https://www.nuget.org/packages/IO.SDK.Net/).
You can install it in your project with this command :
```
dotnet add package IO.SDK.Net
```
## Quick start
```C#
using IO.SDK.Net.DTO;

//Instanciate API
var api = new IO.SDK.Net.API();

//Load kernels from directory, in this example the directory must contain at least the leapseconds kernel file
api.LoadKernels("/home/spacer/Sources/SDK.Net/IO.SDK.Net.Tests/bin/Release/net6.0/Data/SolarSystem");

//Convert elapsed seconds from J2000 to human readable string
var epoch = api.TDBToString(0.0);
Console.WriteLine(epoch); //Expected output : 2000-01-01 12:00:00.000000 (TDB)


```

## Documentation
For more information you can read the [wiki](https://github.com/IO-Aerospace-software-engineering/SDK.Net/wiki)
