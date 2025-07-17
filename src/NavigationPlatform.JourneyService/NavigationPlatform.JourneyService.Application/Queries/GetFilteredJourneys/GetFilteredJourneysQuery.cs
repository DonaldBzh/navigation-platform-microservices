using MediatR;
using NavigationPlatform.JourneyService.Application.Queries.GetFilteredJourneys;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneys;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.Queries.GetFilteredJourneys;

public class GetFilteredJourneysQuery : IRequest<PaginatedResult<JourneyResponse>>
{
    public GetFilteredJourneysRequest Filter { get; set; }

    public GetFilteredJourneysQuery(GetFilteredJourneysRequest filter)
    {
        Filter = filter;
    }

}

