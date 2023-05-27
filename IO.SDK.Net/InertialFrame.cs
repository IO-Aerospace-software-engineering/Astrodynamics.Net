using System.ComponentModel;

namespace IO.SDK.Net;

public enum InertialFrame
{
    [Description("J2000")]
    ICRF,
    [Description("ECLIPJ2000")]
    Ecliptic,
    [Description("GALACTIC")]
    Galactic
}