// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;

    

namespace IO.Astrodynamics.Math;

public class LegendreFunctions
{
    static double Factorial(int n)
    {
        if (n == 0)
            return 1;
        else
            return n * Factorial(n - 1);
    }

    static double PnmFull(int n, int m, double cosTheta)
    {
        double sinTheta = System.Math.Sqrt(1 - cosTheta * cosTheta);

        double term1 = System.Math.Sqrt((2 * n + 1) * Factorial(n - m) / (4 * System.Math.PI * Factorial(n + m)));
        double term2 = System.Math.Pow(sinTheta, m);

        double result = term1 * term2 * AssociatedLegendre(n, m, cosTheta);

        return result;
    }

    static double AssociatedLegendre(int n, int m, double cosTheta)
    {
        if (m == 0)
        {
            return Legendre(n, cosTheta);
        }
        else
        {
            double part1 = System.Math.Sqrt((2 * n + 1) * Factorial(n - m) / (2 * Factorial(n + m)));
            double part2 = System.Math.Pow(cosTheta, m);
            double part3 = Legendre(n, cosTheta);
            double part4 = System.Math.Pow(1 - cosTheta * cosTheta, m / 2.0);

            return part1 * part2 * part3 * part4;
        }
    }

    static double Legendre(int n, double x)
    {
        if (n == 0)
            return 1.0;
        else if (n == 1)
            return x;
        else
            return ((2 * n - 1) * x * Legendre(n - 1, x) - (n - 1) * Legendre(n - 2, x)) / n;
    }
}