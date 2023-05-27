using System;
using System.ComponentModel;
using System.Reflection;

namespace IO.SDK.Net;

public static class Enumeration
{
    public static string GetDescription(this Enum value)
    {            
        FieldInfo field = value.GetType().GetField(value.ToString());

        DescriptionAttribute attribute
            = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                as DescriptionAttribute;

        return attribute == null ? value.ToString() : attribute.Description;
    }
}