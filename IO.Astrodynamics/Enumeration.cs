// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.ComponentModel;

namespace IO.Astrodynamics;

public static class Enumeration
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        var attribute
            = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                as DescriptionAttribute;

        return attribute == null ? value.ToString() : attribute.Description;
    }
}