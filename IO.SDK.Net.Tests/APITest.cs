using System;
using System.Runtime.InteropServices;
using IO.SDK.Net.DTO;
using Xunit;

namespace IO.SDK.Net.Tests;

public class APITest
{
    [Fact]
    public void CheckVersion()
    {
        API api = new API();
        Assert.Equal("CSPICE_N0067", api.GetSpiceVersion());
    }

    [Fact]
    public void Execute()
    {
        API api = new API();
        var scenario = new Scenario();
        scenario.Name = "titi";
        scenario.Window.Start = 10.0;
        scenario.Window.End = 20.0;
        scenario.CelestialBodies = new CelestialBody[10];
        scenario.CelestialBodies[0].Id = 399;
        scenario.CelestialBodies[0].CenterOfMotionId = 10;
        scenario.CelestialBodies[1].Id = 10;
        scenario.Spacecraft.FuelTanks = new FuelTank[5];
        scenario.Spacecraft.FuelTanks[0] = new FuelTank
            { Id = 1, Capacity = 1000.0, Quantity = 1000.0, SerialNumber = "ft1" };
        scenario.Spacecraft.Engines = new EngineDTO[5];
        scenario.Spacecraft.Engines[0] = new EngineDTO
            { Id = 1, Name = "engine1", Fuelflow = 50, SerialNumber = "eng1", FuelTankSerialNumber = "ft1", Isp = 400 };
        scenario.Spacecraft.progradeAttitudes = new ProgradeAttitude[10];
        scenario.Spacecraft.progradeAttitudes[0].Engines = new string[10];
        scenario.Spacecraft.progradeAttitudes[0].Engines[0] = "eng1";
        scenario.Spacecraft.InitialOrbitalParameter.CenterOfMotion.Id = 399;
        scenario.Spacecraft.InitialOrbitalParameter.CenterOfMotion.CenterOfMotionId = 10;
        scenario.Spacecraft.InitialOrbitalParameter.Epoch = 15.0;
        scenario.Spacecraft.InitialOrbitalParameter.Frame = "J2000";
        scenario.Spacecraft.InitialOrbitalParameter.Position = new Vector3D { X = 6800.0, Y = 0.0, Z = 0.0 };
        scenario.Spacecraft.InitialOrbitalParameter.Velocity = new Vector3D { X = 0.0, Y = 8.0, Z = 0.0 };
        scenario.Spacecraft.Id = -1111;
        scenario.Spacecraft.Name = "spc1";
        scenario.Spacecraft.DryOperatingMass = 1000.0;
        scenario.Spacecraft.MaximumOperatingMass = 3000.0;
        API.PropagateProxy(ref scenario);
        // Assert.Equal("titi", res.Name);
        // Assert.Equal(10.0, res.Window.Start);
        // Assert.Equal(20.0, res.Window.End);
        // Assert.Equal("Hello engine", res.Spacecraft.progradeAttitudes[0].Engines[0]);
    }

    [Fact]
    public void CheckSize()
    {
        var scenario = new Scenario();
        var size = Marshal.SizeOf(scenario);
        Assert.Equal(814712, size);
    }
}