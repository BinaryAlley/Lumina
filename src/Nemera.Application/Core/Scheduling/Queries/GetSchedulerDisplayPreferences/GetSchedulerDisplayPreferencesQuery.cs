#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.Scheduling.Queries.GetSchedulerDisplayPreferences;

/// <summary>
/// Query for getting the display preferences of the scheduler page of the current user.
/// </summary>
public record GetSchedulerDisplayPreferencesQuery : IQuery;
