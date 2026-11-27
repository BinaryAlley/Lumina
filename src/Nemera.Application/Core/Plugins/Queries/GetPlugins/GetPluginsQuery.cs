#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetPlugins;

/// <summary>
/// Query for getting all the detected plugins.
/// </summary>
public record GetPluginsQuery() : IQuery;
