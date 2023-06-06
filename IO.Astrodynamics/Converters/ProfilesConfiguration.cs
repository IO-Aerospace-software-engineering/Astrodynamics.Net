// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using AutoMapper;
using IO.Astrodynamics.DTO;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Time;
using StateVector = IO.Astrodynamics.Models.OrbitalParameters.StateVector;

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
        });
        config.AssertConfigurationIsValid();
        Mapper = config.CreateMapper();
    }

    public static ProfilesConfiguration Instance { get; } = new();
}