using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;

public class GetJourneyByIdQuery : IRequest<JourneyResponse>
{
    public Guid Id { get; set; }
}