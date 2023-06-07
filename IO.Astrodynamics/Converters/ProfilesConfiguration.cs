// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using AutoMapper;
using IO.Astrodynamics.DTO;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Surface;
using IO.Astrodynamics.Models.Time;
using Launch = IO.Astrodynamics.Models.Maneuver.Launch;
using Site = IO.Astrodynamics.Models.Surface.Site;
using StateVector = IO.Astrodynamics.Models.OrbitalParameters.StateVector;
using Window = IO.Astrodynamics.Models.Time.Window;

namespace IO.Astrodynamics.Converters;

public class ProfilesConfiguration
{
    public IMapper Mapper { get; }

    private ProfilesConfiguration()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Vector3, Vector3D>().ReverseMap();
            cfg.CreateMap<StateVector, DTO.StateVector>()
                .ForMember(dest => dest.Frame, opt => opt.MapFrom(o => o.Frame.Name))
                .ForMember(dest => dest.Epoch, opt => opt.MapFrom(o => o.Epoch.SecondsFromJ2000()))
                .ForMember(dest => dest.CenterOfMotionId, opt => opt.MapFrom(o => o.CenterOfMotion.PhysicalBody.NaifId));
            cfg.CreateMap<Window, DTO.Window>().ConstructUsing(x => new DTO.Window(x.StartDate.ToTDB().SecondsFromJ2000(), x.EndDate.ToTDB().SecondsFromJ2000()));
            cfg.CreateMap<Models.Coordinates.Geodetic, Geodetic>().ConstructUsing(x => new Geodetic(x.Longitude, x.Latitude, x.Altitude));
            cfg.CreateMap<Site, DTO.Site>()
                .ConstructUsing(x => new DTO.Site(x.NaifId, x.Body.PhysicalBody.NaifId, Mapper.Map<Geodetic>(x.Geodetic), x.Name, x.DirectoryPath.FullName));
            cfg.CreateMap<LaunchSite, DTO.Site>().IncludeBase<Site, DTO.Site>();
            cfg.CreateMap<Launch, DTO.Launch>().ConstructUsing(x => new DTO.Launch(Mapper.Map<DTO.Site>(x.LaunchSite), Mapper.Map<DTO.Site>(x.RecoverySite), x.LaunchByDay ?? false,
                Parameters.FindLaunchResolution.TotalSeconds, Mapper.Map<DTO.StateVector>(x.TargetOrbit.ToStateVector()), new DTO.Window()));
        });
        config.AssertConfigurationIsValid();
        Mapper = config.CreateMapper();
    }

    public static ProfilesConfiguration Instance { get; } = new();
}