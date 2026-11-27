#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.Scheduling.Queries.GetSchedulerDisplayPreferences;

/// <summary>
/// Query for getting the display preferences of the scheduler page of the current user.
/// </summary>
public record GetSchedulerDisplayPreferencesQuery : IQuery;
