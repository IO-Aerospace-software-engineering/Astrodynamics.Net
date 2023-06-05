using Xunit;
using System;
using IO.Astrodynamics.Models.Body.Spacecraft;

namespace IO.Astrodynamics.Models.Tests.Body
{
    public class PayloadTests
    {
        [Fact]
        public void Create()
        {
            Payload payload = new Payload("pl", 1000.0);
            Assert.Equal("pl", payload.Name);
            Assert.Equal(1000.0, payload.Mass);
        }

        [Fact]
        public void CreateInvalid()
        {
            Assert.Throws<ArgumentException>(() => new Payload("", 1000.0));
            Assert.Throws<ArgumentException>(() => new Payload("pl", 0.0));
        }
    }
}