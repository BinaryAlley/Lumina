#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.UsersManagement.Settings.Queries.GetUserSettings;

/// <summary>
/// Query for getting the settings of the current user.
/// </summary>
public record GetUserSettingsQuery : IQuery;
