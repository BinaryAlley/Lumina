#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Plugins;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetPlugins;

/// <summary>
/// Query for getting all the detected plugins.
/// </summary>
public record GetPluginsQuery() : IRequest<ErrorOr<IReadOnlyList<PluginResponse>>>;
