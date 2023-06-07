using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Math
{
    public class Lagrange
    {
        public static double Interpolate((double x, double y)[] data, double idx)
        {
            int n = data.Length;
            double result = 0; // Initialize result

            for (int i = 0; i < n; i++)
            {
                // Compute individual terms
                // of above formula
                double term = data[i].y;
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                        term = term * (idx - data[j].x) / (data[i].x - data[j].x);
                }
                // Add current term to result
                result += term;
            }
            return result;
        }

        public static StateVector Interpolate(StateVector[] data, DateTime epoch)
        {
            double idx = epoch.SecondsFromJ2000();
            int n = data.Length;
            StateVector result = new StateVector(new Vector3(), new Vector3(), data[0].CenterOfMotion, epoch, data[0].Frame); // Initialize result

            for (int i = 0; i < n; i++)
            {
                // Compute individual terms
                // of above formula
                Vector3 posTerm = data[i].Position;
                Vector3 velTerm = data[i].Velocity;
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                    {
                        var t = (idx - data[j].Epoch.SecondsFromJ2000()) / (data[i].Epoch.SecondsFromJ2000() - data[j].Epoch.SecondsFromJ2000());
                        posTerm *= t;
                        velTerm *= t;
                    }
                }
                // Add current term to result
                result.Position += posTerm;
                result.Velocity += velTerm;
            }
            return result;
        }

        public static StateOrientation Interpolate(StateOrientation[] data, DateTime epoch)
        {
            double idx = epoch.SecondsFromJ2000();
            int n = data.Length;
            Quaternion qRes = new Quaternion();
            Vector3 avRes = new Vector3();

            for (int i = 0; i < n; i++)
            {
                // Compute individual terms
                // of above formula
                var q = data[i].Rotation;
                var av = data[i].AngularVelocity;
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                    {
                        var t = (idx - data[j].Epoch.SecondsFromJ2000()) / (data[i].Epoch.SecondsFromJ2000() - data[j].Epoch.SecondsFromJ2000());
                        q *= t;
                        av *= t;
                    }
                }

                // Add current term to result
                qRes *= q;
                avRes += av;
            }
            return new StateOrientation(qRes, avRes, epoch, data[0].ReferenceFrame);
        }
    }
}