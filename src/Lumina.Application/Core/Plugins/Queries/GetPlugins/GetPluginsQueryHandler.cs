#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetPlugins;

/// <summary>
/// Handler for the query to get all the detected plugins.
/// </summary>
public class GetPluginsQueryHandler : IQueryHandler<GetPluginsQuery, Result<IReadOnlyList<PluginResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetPluginsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get all the detected plugins.
    /// </summary>
    /// <param name="query">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="PluginResponse"/>, or an error.
    /// </returns>
    public async Task<Result<IReadOnlyList<PluginResponse>>> HandleAsync(GetPluginsQuery query, CancellationToken cancellationToken)
    {
        IPluginRepository pluginRepository = _unitOfWork.GetRepository<IPluginRepository>();
        Result<IEnumerable<PluginEntity>> getPluginsResult = await pluginRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return getPluginsResult.Match(
            plugins => Result.From<IReadOnlyList<PluginResponse>>(plugins.Select(plugin => plugin.ToResponse()).ToList()),
            errors => errors);
    }
}
