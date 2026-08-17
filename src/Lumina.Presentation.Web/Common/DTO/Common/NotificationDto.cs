#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Notifications;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Common;

/// <summary>
/// Data transfer object for notifications.
/// </summary>
/// <param name="Id">The id of the notification.</param>
/// <param name="Message">The message of the notification.</param>
/// <param name="Type">The type of the notification.</param>
[DebuggerDisplay("Id: {Id}, Message: {Message}")]
public record struct NotificationDto(
    Guid Id, 
    string Message, 
    NotificationType Type
);
