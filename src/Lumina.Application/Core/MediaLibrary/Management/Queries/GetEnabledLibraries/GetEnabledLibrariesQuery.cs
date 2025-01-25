#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetEnabledLibraries;

/// <summary>
/// Query for getting the list of enabled media libraries.
/// </summary>
public record GetEnabledLibrariesQuery : IRequest<ErrorOr<LibraryResponse[]>>;
