// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

namespace IO.Astrodynamics.Cosmographia.Models;

public class SpiceRootObject
{
    public string version { get; set; }
    public string name { get; set; }
    public string[] spiceKernels { get; set; }
}

