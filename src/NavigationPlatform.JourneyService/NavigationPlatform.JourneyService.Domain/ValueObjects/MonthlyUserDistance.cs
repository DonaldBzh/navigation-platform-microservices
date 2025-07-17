using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Domain.ValueObjects;

public class MonthlyUserDistance
{
    public Guid UserId { get; set; }   
    public int Year { get; set; }                          
    public int Month { get; set; }                 
    public decimal TotalDistanceKm { get; set; }
}
