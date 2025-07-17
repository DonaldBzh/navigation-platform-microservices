using MediatR;
using NavigationPlatform.JourneyService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.Queries.GetPublicJourney;

public record GetPublicJourneyQuery(string Token) : IRequest<PublicJourneyResponse>;
