namespace Simple.Messaging;

internal class EventHandlerRegistration(Type eventType, Delegate eventHandler)
{
    internal Type EventType { get; } = eventType;
    internal Delegate EventHandler { get; } = eventHandler;
}