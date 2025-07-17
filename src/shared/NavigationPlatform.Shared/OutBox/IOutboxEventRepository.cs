using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Shared.OutBox;

public interface IOutboxEventRepository
{
    Task AddAsync(OutboxEvent outboxEvent);
}
