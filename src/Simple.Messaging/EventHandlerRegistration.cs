namespace Simple.Messaging;

internal class EventHandlerRegistration(Guid id, Type eventType, Delegate eventHandler)
{
    internal Guid Id { get; } = id;
    internal Type EventType { get; } = eventType;
    internal Delegate EventHandler { get; } = eventHandler;
}