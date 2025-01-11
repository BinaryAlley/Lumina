#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraries;

/// <summary>
/// Query for getting the list of media libraries.
/// </summary>
public record GetLibrariesQuery : IRequest<ErrorOr<LibraryResponse[]>>;
