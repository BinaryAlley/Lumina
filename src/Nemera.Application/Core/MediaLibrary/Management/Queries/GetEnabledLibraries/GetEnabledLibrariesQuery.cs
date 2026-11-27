#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Queries.GetEnabledLibraries;

/// <summary>
/// Query for getting the list of enabled media libraries.
/// </summary>
public record GetEnabledLibrariesQuery : IQuery;
