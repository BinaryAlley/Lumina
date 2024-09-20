#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Notifications;
using Lumina.Presentation.Web.Common.Models.Common;
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Service for showing notifications.
/// </summary>
public class NotificationService : INotificationService
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    public event Action<NotificationModel>? OnNotification;
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Shows a notification.
    /// </summary>
    /// <param name="message">The message of the notification.</param>
    /// <param name="type">The type of the notification.</param>
    public void Show(string message, NotificationType type)
    {
        OnNotification?.Invoke(new NotificationModel { Message = message, Type = type, Id = Guid.NewGuid() });
    }
    #endregion
}