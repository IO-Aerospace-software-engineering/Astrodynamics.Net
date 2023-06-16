using System;
using IO.Astrodynamics.Body.Spacecraft;
using Xunit;

namespace IO.Astrodynamics.Tests.Body
{
    public class PayloadTests
    {
        [Fact]
        public void Create()
        {
            Payload payload = new Payload("pl", 1000.0,"sn");
            Assert.Equal("pl", payload.Name);
            Assert.Equal(1000.0, payload.Mass);
            Assert.Equal("sn", payload.SerialNumber);
        }

        [Fact]
        public void CreateInvalid()
        {
            Assert.Throws<ArgumentException>(() => new Payload("", 1000.0,"sn"));
            Assert.Throws<ArgumentException>(() => new Payload("pl", 0.0,"sn"));
            Assert.Throws<ArgumentException>(() => new Payload("pl", 1000.0,""));
        }
    }
}