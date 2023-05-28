using System.ComponentModel;

namespace IO.SDK.Net;

public enum CoordinateSystem
{
    [Description("RECTANGULAR")]
    Rectangular,
    [Description("LATITUDINAL")]
    Latitudinal,
    [Description("RA/DEC")]
    RaDec,
    [Description("SPHERICAL")]
    Spherical,
    [Description("CYLINDRICAL")]
    Cylindrical,
    [Description("GEODETIC")]
    Geodetic,
    [Description("PLANETOGRAPHIC")]
    Planetographic

}