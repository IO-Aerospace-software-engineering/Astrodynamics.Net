using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.OrbitalParameters
{
    public class StateOrientationTests
    {
        [Fact]
        public void Create()
        {
            var so = new StateOrientation(new Quaternion(1, 2, 3, 4), Vector3.VectorX, DateTime.MaxValue, IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Assert.NotNull(so);
            Assert.Equal(new Quaternion(1, 2, 3, 4), so.Orientation);
            Assert.Equal(Vector3.VectorX, so.AngularVelocity);
            Assert.Equal(DateTime.MaxValue, so.Epoch);
            Assert.Equal(IO.Astrodynamics.Models.Frame.Frame.ICRF, so.Frame);
        }

        [Fact]
        public void RelativeToICRF()
        {
            var so = new StateOrientation(new Quaternion(Vector3.VectorX, 10.0 * Constants.Deg2Rad),Vector3.Zero, DateTime.MaxValue, IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            var res = so.RelativeToICRF();
            Assert.NotNull(so);

            //Which is equal to ecliptic (23.44° + 10° relative to ecliptic)
            Assert.Equal(new Quaternion(0.95772390696309828, 0.28768892166113869, 0, 0), res.Orientation);
            Assert.Equal(DateTime.MaxValue, res.Epoch);
            Assert.Equal(IO.Astrodynamics.Models.Frame.Frame.ICRF, res.Frame);
        }
    }
}
