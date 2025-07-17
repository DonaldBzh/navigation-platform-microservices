using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.Commands.SharePublic;

public record CreatePublicShareLinkCommand(Guid JourneyId, DateTime? ExpiresAt) : IRequest<string>;
