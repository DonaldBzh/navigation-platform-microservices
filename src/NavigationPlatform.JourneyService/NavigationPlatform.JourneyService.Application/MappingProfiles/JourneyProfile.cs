using AutoMapper;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.MappingProfiles;

public class JourneyProfile : Profile
{
    public JourneyProfile()
    {
        CreateMap<Journey, JourneyResponse>()
              .ForMember(d => d.TransportationType,
                  opt => opt.MapFrom(s => s.TransportationType.ToString()));
    }
}
