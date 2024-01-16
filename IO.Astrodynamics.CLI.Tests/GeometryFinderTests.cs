using System.Text;
using IO.Astrodynamics.CLI.Commands;
using IO.Astrodynamics.CLI.Commands.Parameters;

namespace IO.Astrodynamics.CLI.Tests;

public class GeometryFinderTests
{
    [Fact]
    public void CoordinateConstraint()
    {
        lock (Configuration.objLock)
        {
            var command = new GeometryFinderCommand();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Console.SetOut(sw);
            command.CoordinateConstraint("Data", 10, 399, "Rectangular", "Z", "=", 0.0,  new WindowParameters { Begin = new EpochParameters{Epoch = "2024-03-01T00:00:00"}, End = new EpochParameters{Epoch = "2024-05-01T00:00:00" }},
                TimeSpan.FromDays(20), frame: "ECLIPJ2000");
            var res = sb.ToString();

            Assert.Equal(
                $"From 2024-03-17T05:19:00.9354395 (TDB) to 2024-03-17T05:19:00.9354395 (TDB) - Length 00:00:00{Environment.NewLine}"
                , res);
        }
    }
    
    [Fact]
    public void DistanceConstraint()
    {
        lock (Configuration.objLock)
        {
            var command = new GeometryFinderCommand();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Console.SetOut(sw);
            command.DistanceConstraint("Data", 399, 301, "=", 400000000.0, new WindowParameters { Begin = new EpochParameters{Epoch = "2024-03-01T00:00:00"}, End = new EpochParameters{Epoch = "2024-03-21T00:00:00" }},
                TimeSpan.FromDays(10));
            var res = sb.ToString();

            Assert.Equal(
                $"From 2024-03-19T18:04:11.9364664 (TDB) to 2024-03-19T18:04:11.9364664 (TDB) - Length 00:00:00{Environment.NewLine}"
                , res);
        }
    }
}