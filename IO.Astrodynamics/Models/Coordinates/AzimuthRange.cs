﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Coordinates
{
    public readonly record struct AzimuthRange
    {
        public double Start { get; }
        public double End { get; }
        public double Span { get; }

        [JsonConstructor]
        public AzimuthRange(double start, double end)
        {
            Start = start % Constants._2PI;
            if (Start < 0.0)
            {
                Start += Constants._2PI;
            }

            End = end % Constants._2PI;
            if (End < 0.0)
            {
                End += Constants._2PI;
            }

            Span = End - Start;
            if (Span < 0.0)
            {
                Span += Constants._2PI;
            }
        }

        public readonly bool IsInRange(double angle)
        {
            var a = angle - Start % Constants._2PI;
            if (a < 0.0)
            {
                a += Constants._2PI;
            }

            var end = End;

            if (end < Start)
            {
                end += Constants._2PI;
            }

            end = end - Start;

            if (end < 0.0)
            {
                end += Constants._2PI;
            }

            if (a >= 0.0 && a <= end)
            {
                return true;
            }

            return false;
        }

        public bool IsIntersected(in AzimuthRange azimuthRange)
        {
            return IsInRange(azimuthRange.Start) || IsInRange(azimuthRange.End) || azimuthRange.IsInRange(Start) ||
                   azimuthRange.IsInRange(End);
        }
    }
}