using System;
using System.IO;

namespace IO.Astrodynamics.Tests;

public class Constants
{
    public const double RAD_DEG = 180.0 / Math.PI;
    public const double DEG_RAD = Math.PI / 180.0;
    public static readonly DirectoryInfo SpacecraftPath = new("Data/User/Spacecrafts");
    public static readonly DirectoryInfo SolarSystemKernelPath = new("Data/SolarSystem");
    public static readonly DirectoryInfo SitePath = new("Data/User/Sites");
}