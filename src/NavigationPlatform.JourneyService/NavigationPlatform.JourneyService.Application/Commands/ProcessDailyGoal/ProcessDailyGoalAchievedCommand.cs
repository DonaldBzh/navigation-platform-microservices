using MediatR;
using NavigationPlatform.JourneyService.Application.Commands.RevokePublic;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Events;
using NavigationPlatform.Shared.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Application.Commands.ProcessDailyGoal;

public record ProcessDailyGoalAchievedCommand(DailyGoalAchieved DailyGoal) : IRequest;
