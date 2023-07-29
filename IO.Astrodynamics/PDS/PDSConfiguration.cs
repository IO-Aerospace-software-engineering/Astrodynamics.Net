// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace IO.Astrodynamics.PDS;

public class PDSConfiguration
{
    private readonly HashSet<(string nms,Stream stream)> _schemas;
    public IReadOnlyCollection<(string nms,Stream stream)> Schemas => _schemas;

    public PDSConfiguration(params (string nms,Stream stream)[] schemas)
    {
        _schemas = new HashSet<(string nms,Stream stream)>(schemas);
    }
}