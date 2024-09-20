#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Notifications;
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Common;

/// <summary>
/// Model for notifications.
/// </summary>
/// <param name="Id">The id of the notification.</param>
/// <param name="Message">The message of the notification.</param>
/// <param name="Type">The type of the notification.</param>
public record struct NotificationModel(Guid Id, string Message, NotificationType Type);