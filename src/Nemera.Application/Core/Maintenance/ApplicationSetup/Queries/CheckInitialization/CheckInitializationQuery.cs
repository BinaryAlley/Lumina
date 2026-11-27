#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;

/// <summary>
/// Query for checking the initialization of the application.
/// </summary>
public record CheckInitializationQuery() : IQuery;
