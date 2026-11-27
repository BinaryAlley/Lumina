#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraries;

/// <summary>
/// Query for getting the list of media libraries.
/// </summary>
public record GetLibrariesQuery : IQuery;
