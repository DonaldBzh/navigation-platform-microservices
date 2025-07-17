using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Shared.OutBox;

public class OutboxEvent
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = null!;
    public string EventData { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsProcessed { get; set; } = false;
    public DateTime? ProcessedAt { get; set; }

}
