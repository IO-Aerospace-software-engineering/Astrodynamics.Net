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
        scenario.Spacecraft.progradeAttitudes = new ProgradeAttitude[10];
        scenario.Spacecraft.progradeAttitudes[0].Engines = new String[10];
        scenario.Spacecraft.progradeAttitudes[0].Engines[0] = "Hello engine";
        scenario.Spacecraft.InitialOrbitalParameter.CenterOfMotion.Id = 399;
        scenario.Spacecraft.InitialOrbitalParameter.Epoch = 15.0;
        scenario.Spacecraft.InitialOrbitalParameter.Frame = "J2000";
        scenario.Spacecraft.InitialOrbitalParameter.Position = new Vector3D { X = 1.0, Y = 2.0, Z = 3.0 };
        scenario.Spacecraft.Id = -1111;
        scenario.Spacecraft.Name = "spc1";
        scenario.Spacecraft.DryOperatingMass = 1000.0;
        scenario.Spacecraft.MaximumOperatingMass = 3000.0;
        var res = api.ExecuteScenario(scenario);
        Assert.Equal("titi", res.Name);
        Assert.Equal(10.0, res.Window.Start);
        Assert.Equal(20.0, res.Window.End);
        Assert.Equal("Hello engine", res.Spacecraft.progradeAttitudes[0].Engines[0]);
    }

    [Fact]
    public void CheckSize()
    {
        // var apogee = new ApogeeHeightChangingManeuver();
        // var size = Marshal.SizeOf(apogee);
        // Assert.Equal(2160664,size);

        var spacecraft = new Spacecraft();
        Assert.Equal(123, Marshal.SizeOf(spacecraft));
        var scenario = new Scenario();
        var size = Marshal.SizeOf(scenario);
        Assert.Equal(2160664, size);
    }

    [Fact]
    public void GetValue()
    {
        API api = new API();
        Assert.Equal(0, api.GetValue());
        Assert.Equal(1, api.GetValue());

        API api2 = new API();
        Assert.Equal(0, api2.GetValue());
        Assert.Equal(1, api2.GetValue());
    }
}