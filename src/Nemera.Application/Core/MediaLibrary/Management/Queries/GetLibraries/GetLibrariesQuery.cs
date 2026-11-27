#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Queries.GetLibraries;

/// <summary>
/// Query for getting the list of media libraries.
/// </summary>
public record GetLibrariesQuery : IQuery;
