using System.Reflection;

namespace Simple.Messaging;

public class InProcessEventService(IServiceProvider? serviceProvider = null) : IEventService
{
    private List<EventHandlerRegistration> subscriptions = [];

    public void Handle(IEvent @event)
    {
        var eventTypes = GetTypesImplementingIEvent(@event);
        var matchedSubscriptions = subscriptions
            .Where(subscription => eventTypes.Contains(subscription.EventType))
            .ToArray();

        foreach (var eventHandler in matchedSubscriptions.Select(s => s.EventHandler))
        {
            var arguments = ResolveArguments(@event, eventHandler, eventTypes);
            eventHandler.DynamicInvoke(arguments);
        }
    }

    public Guid Register<TEvent>(Delegate eventHandler) where TEvent : IEvent
    {
        EventHandlerRegistration subscription = new(Guid.NewGuid(), typeof(TEvent), eventHandler);
        subscriptions.Add(subscription);
        return subscription.Id;
    }

    public void Unregister(Guid subscriptionId)
    {
        var subscription = subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        if (subscription != null)
        {
            subscriptions.Remove(subscription);
        }
    }

    private object[] ResolveArguments(IEvent @event, Delegate eventHandler, Type[] eventTypes)
    {
        List<object> arguments = [];
        var methodInfo = eventHandler.GetMethodInfo();
        var parameters = methodInfo.GetParameters();

        foreach (var parameterType in parameters.Select(p => p.ParameterType))
        {
            if (eventTypes.Contains(parameterType))
            {
                arguments.Add(@event);
            }
            else
            {
                if (serviceProvider != null)
                {
                    var argument = serviceProvider.GetService(parameterType);
                    arguments.Add(argument ?? new ArgumentNullException("InMemeorySubscriptionService Could not resolve dependency " + parameterType.Name));
                }
                else
                {
                    throw new ArgumentNullException("InMemeorySubscriptionService: serviceProvider null constructor argument!");
                }
            }
        }

        return arguments.ToArray();
    }

    private static Type[] GetTypesImplementingIEvent(IEvent @event)
        => @event.GetType()
        .GetInterfaces()
        .Where(type => type.GetInterfaces().Contains(typeof(IEvent)) || type.Equals(typeof(IEvent)))
        .Concat([@event.GetType()])
        .Concat(GetAllBaseTypes(@event.GetType()).Where(type => type.GetInterfaces().Contains(typeof(IEvent))))
        .ToArray();

    private static Type[] GetAllBaseTypes(Type type)
    {
        List<Type> baseTypes = [];
        Type? baseType = type.BaseType;
        while (baseType != null)
        {
            baseTypes.Add(baseType);
            baseType = baseType.BaseType;
        }

        return baseTypes.ToArray();
    }
}