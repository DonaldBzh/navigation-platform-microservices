using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;

namespace NavigationPlatform.JourneyService.Application.Queries.GetFilteredJourneys
{
    public class GetFilteredJourneysRequest
    {
        public Guid? UserId { get; set; }
        public TransportationType? TransportationType { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? ArrivalDateFrom { get; set; }
        public DateTime? ArrivalDateTo { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string OrderBy { get; set; } = nameof(Journey.CreatedAt);
        public string Direction { get; set; } = "DESC";
    }
}
