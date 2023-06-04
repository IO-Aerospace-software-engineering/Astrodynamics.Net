// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.ComponentModel;

namespace IO.SDK.Net;

public enum IlluminationAngle
{
    [Description("PHASE")] Phase,
    [Description("INCIDENCE")] Incidence,
    [Description("EMISSION")] Emission
}