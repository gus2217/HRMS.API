using Jacana.SharedKernel.Application.Abstractions;

namespace Jacana.SharedKernel.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
