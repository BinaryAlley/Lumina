#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Notifications;
using Lumina.Presentation.Web.Common.Models.Common;
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Interface for the service for showing notifications.
/// </summary>
public interface INotificationService
{
    event Action<NotificationModel>? OnNotification;

    /// <summary>
    /// Shows a notification.
    /// </summary>
    /// <param name="message">The message of the notification.</param>
    /// <param name="type">The type of the notification.</param>
    void Show(string message, NotificationType type);
}