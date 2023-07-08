// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using AutoMapper;
using IO.Astrodynamics.DTO;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Surface;
using IO.Astrodynamics.Time;
using Launch = IO.Astrodynamics.Maneuver.Launch;
using Planetodetic = IO.Astrodynamics.Coordinates.Planetodetic;
using Quaternion = IO.Astrodynamics.Math.Quaternion;
using Site = IO.Astrodynamics.Surface.Site;
using StateOrientation = IO.Astrodynamics.OrbitalParameters.StateOrientation;
using StateVector = IO.Astrodynamics.OrbitalParameters.StateVector;
using Window = IO.Astrodynamics.Time.Window;

namespace IO.Astrodynamics.Converters;

public class ProfilesConfiguration
{
    public IMapper Mapper { get; }

    private ProfilesConfiguration()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Vector3, Vector3D>().ReverseMap();
            cfg.CreateMap<Quaternion, DTO.Quaternion>().ConstructUsing(x => new DTO.Quaternion(x.W, x.VectorPart.X, x.VectorPart.Y, x.VectorPart.Z))
                .ForMember(x => x.W, o => o.Ignore())
                .ForMember(x => x.X, o => o.Ignore())
                .ForMember(x => x.Y, o => o.Ignore())
                .ForMember(x => x.Z, o => o.Ignore());
            cfg.CreateMap<DTO.Quaternion, Quaternion>().ConstructUsing(x => new Quaternion(x.W, x.X, x.Y, x.Z));

            cfg.CreateMap<StateVector, DTO.StateVector>().ConstructUsing(x => new DTO.StateVector(x.Observer.NaifId, x.Epoch.SecondsFromJ2000TDB(), x.Frame.Name,
                Mapper.Map<Vector3D>(x.Position), Mapper.Map<Vector3D>(x.Velocity)));

            cfg.CreateMap<StateOrientation, DTO.StateOrientation>()
                .ForMember(x => x.Rotation, o => o.MapFrom(x => x.Rotation))
                .ForMember(x => x.AngularVelocity, o => o.MapFrom(x => x.AngularVelocity))
                .ForMember(x => x.Epoch, o => o.MapFrom(x => x.Epoch.SecondsFromJ2000TDB()))
                .ForMember(x => x.Frame, o => o.MapFrom(x => x.ReferenceFrame.Name));

            cfg.CreateMap<Window, DTO.Window>().ConstructUsing(x => new DTO.Window(x.StartDate.ToTDB().SecondsFromJ2000TDB(), x.EndDate.ToTDB().SecondsFromJ2000TDB()));
            cfg.CreateMap<DTO.Window, Window>().ConstructUsing(x => new Window(DateTimeExtension.CreateTDB(x.Start), DateTimeExtension.CreateTDB(x.End)));

            cfg.CreateMap<Planetodetic, DTO.Planetodetic>().ConstructUsing(x => new DTO.Planetodetic(x.Longitude, x.Latitude, x.Altitude));

            cfg.CreateMap<Site, DTO.Site>()
                .ConstructUsing(x => new DTO.Site(x.NaifId, x.Body.NaifId, Mapper.Map<DTO.Planetodetic>(x.Planetodetic), x.Name, string.Empty))
                .ForMember(x => x.BodyId, o => o.MapFrom(x => x.Body.NaifId))
                .ForMember(x => x.Coordinates, o => o.MapFrom(x => x.Planetodetic))
                .ForMember(x => x.Ranges, o => o.Ignore())
                .ForMember(x => x.Id, o => o.MapFrom(x => x.NaifId))
                .ForMember(x => x.DirectoryPath, o => o.Ignore());
            cfg.CreateMap<LaunchSite, DTO.Site>().IncludeBase<Site, DTO.Site>();

            cfg.CreateMap<Launch, DTO.Launch>().ConstructUsing(x => new DTO.Launch(Mapper.Map<DTO.Site>(x.LaunchSite), Mapper.Map<DTO.Site>(x.RecoverySite), x.LaunchByDay ?? false,
                    Parameters.FindLaunchResolution.TotalSeconds, Mapper.Map<StateVector, DTO.StateVector>(x.TargetOrbit.ToStateVector()), new DTO.Window()))
                .ForMember(x => x.Window, o => o.Ignore())
                .ForMember(x => x.InitialStepSize, o => o.Ignore())
                .ForMember(x => x.InertialAzimuth, o => o.Ignore())
                .ForMember(x => x.InertialInsertionVelocity, o => o.Ignore())
                .ForMember(x => x.NonInertialAzimuth, o => o.Ignore())
                .ForMember(x => x.NonInertialInsertionVelocity, o => o.Ignore())
                .ForMember(x => x.Windows, o => o.Ignore())
                .ForMember(x => x.TargetOrbit, o => o.Ignore());
        });
        config.AssertConfigurationIsValid();
        Mapper = config.CreateMapper();
    }

    public static ProfilesConfiguration Instance { get; } = new();
}