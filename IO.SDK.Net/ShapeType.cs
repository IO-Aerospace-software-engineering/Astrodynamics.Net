// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.ComponentModel;

namespace IO.SDK.Net;

public enum ShapeType
{
    [Description("ELLIPSOID")] Ellipsoid,
    [Description("POINT")] Point
}