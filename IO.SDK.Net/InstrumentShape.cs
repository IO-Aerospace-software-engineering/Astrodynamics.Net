// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.ComponentModel;

namespace IO.SDK.Net;

public enum InstrumentShape
{
    [Description("rectangular")] Circular,
    [Description("elliptical")] Elliptical,
    [Description("rectangular")] Rectangular
}