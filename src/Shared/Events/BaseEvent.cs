namespace Shared.Events;

public abstract record BaseEvent
{
    public DateTime Timestamp { get; init; }
}
