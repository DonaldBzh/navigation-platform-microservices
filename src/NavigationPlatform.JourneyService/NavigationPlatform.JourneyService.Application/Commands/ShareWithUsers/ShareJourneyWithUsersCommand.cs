using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.Commands.ShareWithUsers;

public record ShareJourneyWithUsersCommand(Guid JourneyId, List<Guid> UserIds) : IRequest;
