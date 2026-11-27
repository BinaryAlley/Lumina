#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.Plugins.Queries.GetPlugins;

/// <summary>
/// Query for getting all the detected plugins.
/// </summary>
public record GetPluginsQuery() : IQuery;
