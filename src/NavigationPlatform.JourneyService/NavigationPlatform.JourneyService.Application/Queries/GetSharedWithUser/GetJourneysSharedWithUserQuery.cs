using MediatR;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.Queries.GetSharedWithUser;

public record GetJourneysSharedWithUserQuery() : IRequest<List<JourneyResponse>>;
