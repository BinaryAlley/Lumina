#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries;

/// <summary>
/// Query for validating a path.
/// </summary>
/// <param name="Path">The path to be validated.</param>
public record ValidatePathQuery(string Path) : IRequest<PathValidResponse>;