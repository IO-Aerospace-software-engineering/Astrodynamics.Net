using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.SeedWork;

namespace IO.Astrodynamics.Models.Body;



public abstract class Body : Entity, INaifObject
{
    public int NaifId { get; private set; }
    public string Name { get; private set; }
    public double Mass { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="naifId"></param>
    /// <param name="name"></param>
    /// <param name="mass"></param>
    protected Body(int naifId, string name, double mass)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Body must have a name");
        }

        if (mass <= 0)
        {
            throw new ArgumentException("Body must have a mass");
        }

        Id = naifId;
        NaifId = naifId;
        Name = name;
        Mass = mass;
    }

    public override string ToString()
    {
        return Name;
    }

    public abstract double GetTotalMass();

    public override bool Equals(object obj)
    {
        return Equals(obj as Body);
    }

    public bool Equals(Body other)
    {
        return base.Equals(other) ||
               NaifId == other.NaifId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), NaifId);
    }

    public static bool operator ==(Body left, Body right)
    {
        return EqualityComparer<Body>.Default.Equals(left, right);
    }

    public static bool operator !=(Body left, Body right)
    {
        return !(left == right);
    }
}