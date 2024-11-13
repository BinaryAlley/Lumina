#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.UsersManagement;
using Mediator;
#endregion

namespace Lumina.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;

/// <summary>
/// Query for checking the initialization of the application.
/// </summary>
public record CheckInitializationQuery() : IRequest<InitializationResponse>;
