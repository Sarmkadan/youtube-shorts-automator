// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Events;

/// <summary>
/// Event publisher for in-process event distribution
/// Supports async event handling with exception aggregation
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent;
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent;
}

public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredAtUtc { get; }
    string EventType { get; }
}

public interface IEventHandler<TEvent> where TEvent : class, IEvent
{
    Task HandleAsync(TEvent @event);
}

public class EventPublisher : IEventPublisher
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(ILogger<EventPublisher> logger)
    {
        _logger = logger;
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        var eventType = typeof(TEvent);

        if (!_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType] = new List<Delegate>();
        }

        _subscribers[eventType].Add(handler.HandleAsync);
        _logger.LogDebug("Subscriber registered for event: {EventType}", eventType.Name);
    }

    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        var eventType = typeof(TEvent);

        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            var methodToRemove = handlers.FirstOrDefault(h => h.Target == handler);
            if (methodToRemove != null)
            {
                handlers.Remove(methodToRemove);
                _logger.LogDebug("Subscriber unregistered for event: {EventType}", eventType.Name);
            }
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        var eventType = typeof(TEvent);

        _logger.LogInformation("Publishing event: {EventType} (EventId: {EventId})",
            @event.EventType, @event.EventId);

        if (!_subscribers.TryGetValue(eventType, out var handlers) || handlers.Count == 0)
        {
            _logger.LogWarning("No subscribers found for event: {EventType}", eventType.Name);
            return;
        }

        var tasks = new List<Task>();
        var exceptions = new List<Exception>();

        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Func<TEvent, Task> asyncHandler)
                {
                    tasks.Add(asyncHandler(@event));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking event handler for event: {EventType}", eventType.Name);
                exceptions.Add(ex);
            }
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error awaiting event handlers for event: {EventType}", eventType.Name);
            exceptions.Add(ex);
        }

        if (exceptions.Count > 0)
        {
            _logger.LogError("Event {EventType} processing completed with {ErrorCount} errors",
                eventType.Name, exceptions.Count);
        }
        else
        {
            _logger.LogInformation("Event {EventType} published successfully", eventType.Name);
        }
    }
}
