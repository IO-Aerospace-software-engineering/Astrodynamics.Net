using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace IO.Astrodynamics.PDS.V4.MissionInformation;

public class PDSSurfaceMissionInformation : PDSBase<Mission_Information_1K00_1300>
{
    public PDSSurfaceMissionInformation() : base(new PDSConfiguration(("http://pds.nasa.gov/pds4/pds/v1", Assembly.GetAssembly(typeof(PDSMissionInformation)).GetManifestResourceStream("IO.Astrodynamics.PDS.V4.Schemas.PDS4_PDS_1K00.xsd")),
        ("http://pds.nasa.gov/pds4/msn_surface/v1", Assembly.GetAssembly(typeof(PDSSurfaceMissionInformation)).GetManifestResourceStream("IO.Astrodynamics.PDS.V4.Schemas.PDS4_MSN_SURFACE_1K00_1220.xsd"))))
    {
    }
}