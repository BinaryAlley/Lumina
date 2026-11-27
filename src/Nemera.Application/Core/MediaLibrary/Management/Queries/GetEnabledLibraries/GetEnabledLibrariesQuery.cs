#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetEnabledLibraries;

/// <summary>
/// Query for getting the list of enabled media libraries.
/// </summary>
public record GetEnabledLibrariesQuery : IQuery;
