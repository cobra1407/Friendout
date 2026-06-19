using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Observer pattern — the subject (publisher) of the notification system.
///
/// Maintains a list of INotificationService observers injected via DI.
/// When DispatchNotificationAsync is called, it forwards the event to all
/// observers simultaneously. Callers never know which observers are registered.
///
/// To add a new observer: implement INotificationService and register it in DI.
/// This class never needs to change.
/// </summary>
public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IEnumerable<INotificationService> _observers;

    public NotificationDispatcher(IEnumerable<INotificationService> observers)
    {
        _observers = observers;
    }

    public async Task DispatchNotificationAsync(Guid userId, NotificationType type, Dictionary<string, string> data)
    {
        var tasks = _observers.Select(observer => observer.NotifyUserAsync(userId, type, data));
        await Task.WhenAll(tasks);
    }
}
