using System.ComponentModel;

namespace IO.SDK.Net;

public enum Coordinate
{
    [Description("X")]
    X,
    [Description("Y")]
    Y,
    [Description("Z")]
    Z,
    [Description("ALTITUDE")]
    Altitude,
    [Description("COLATITUDE")]
    Colatitude,
    [Description("DECLINATION")]
    Declination,
    [Description("LATITUDE")]
    Latitude,
    [Description("LONGITUDE")]
    Longitude,
    [Description("RADIUS")]
    Radius,
    [Description("RANGE")]
    Range,
    [Description("RIGHT ASCENSION")]
    RightAscenssion

}