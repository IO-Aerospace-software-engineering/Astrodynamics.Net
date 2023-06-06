using System;
using Xunit;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Tests.Math;

public class VectorTests
{
    [Fact]
    public void Createvector()
    {
        Vector3 m = new Vector3(1, 2, 3);
        Assert.Equal(1, m.X);
        Assert.Equal(2, m.Y);
        Assert.Equal(3, m.Z);
    }

    [Fact]
    public void Magnitude()
    {
        Vector3 m = new Vector3(2, 3, 4);
        Assert.Equal(5.385164807134504, m.Magnitude());
    }

    [Fact]
    public void Multiply()
    {
        Vector3 m = new Vector3(2, 3, 4);
        m *= 2.0;
        Assert.Equal(new Vector3(4, 6, 8), m);
    }

    [Fact]
    public void Add()
    {
        Vector3 m1 = new Vector3(4, 6, 8);
        Vector3 m2 = new Vector3(1, 2, 3);
        m1 += m2;
        Assert.Equal(new Vector3(5, 8, 11), m1);
    }

    [Fact]
    public void Subtract()
    {
        Vector3 m1 = new Vector3(4, 6, 8);
        Vector3 m2 = new Vector3(1, 2, 3);
        m1 -= m2;
        Assert.Equal(new Vector3(3, 4, 5), m1);
    }

    [Fact]
    public void Dot()
    {

        Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
        Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
        Vector3 m1 = new Vector3(2, 3, 4);
        Vector3 m2 = new Vector3(5, 6, 7);
        double res = m1 * m2;
        Assert.Equal(56, res);
    }

    [Fact]
    public void Cross()
    {
        Vector3 m1 = new Vector3(1, 0, 0);
        Vector3 m2 = new Vector3(0, 1, 0);
        Vector3 v = m1.Cross(m2);
        Assert.Equal(new Vector3(0, 0, 1), v);
    }

    [Fact]
    public void Normalize()
    {
        Vector3 m2 = new Vector3(3, 4, 5);
        Vector3 v = m2.Normalize();
        Assert.Equal(1.0, v.Magnitude(), 15);
    }

    [Fact]
    public void Angle()
    {
        Vector3 m1 = new Vector3(1, 0, 0);
        Vector3 m2 = new Vector3(0, 1, 0);
        double angle = m1.Angle(m2);
        Assert.Equal(System.Math.PI / 2.0, angle);
    }

    [Fact]
    public void To()
    {
        Vector3 m1 = new Vector3(10, 0, 0);
        Vector3 m2 = new Vector3(0, 10, 0);
        var q = m1.To(m2);
        Assert.Equal(0.7071067811865475, q.W);
        Assert.Equal(0.0, q.VectorPart.X);
        Assert.Equal(0.0, q.VectorPart.Y);
        Assert.Equal(-0.7071067811865475, q.VectorPart.Z);
    }

    [Fact]
    public void Rotate()
    {
        Vector3 m1 = new Vector3(10, 0, 0);
        Quaternion q = new Quaternion(0.7071067811865475, 0.0, 0.0, 0.7071067811865475);
        var m2 = m1.Rotate(q);
        Assert.Equal(0.0, m2.X);
        Assert.Equal(10.0, m2.Y, 12);
        Assert.Equal(0.0, m2.Z);
    }


}