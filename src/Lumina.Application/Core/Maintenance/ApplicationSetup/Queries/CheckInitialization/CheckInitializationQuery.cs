#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;

/// <summary>
/// Query for checking the initialization of the application.
/// </summary>
public record CheckInitializationQuery() : IQuery;
