#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.UsersManagement.Settings.Queries.GetUserSettings;

/// <summary>
/// Query for getting the settings of the current user.
/// </summary>
public record GetUserSettingsQuery : IQuery;
