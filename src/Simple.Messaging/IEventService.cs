namespace Simple.Messaging;
public interface IEventService
{
    Guid Register<TEvent>(Delegate eventHandler) where TEvent : IEvent;
    void Unregister(Guid eventHandlerId);
    void Handle(IEvent @event);
}
