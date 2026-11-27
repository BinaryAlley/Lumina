#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "get all lite" action for interacting with a generic persistence medium.
/// It retrieves lightweight read models of the stored data, projecting only the fields needed to display
/// the data in grids or card-based navigation, without loading the full entities and their related data.
/// </summary>
/// <typeparam name="TModel">The type stored in the persistence medium, which is projected into the lightweight read models. It should implement <see cref="IStorageEntity"/>.</typeparam>
/// <typeparam name="TLiteRow">The type of the lightweight read models returned by the action.</typeparam>
public interface IGetAllLiteRepositoryAction<TModel, TLiteRow> where TModel : IStorageEntity
{
    /// <summary>
    /// Gets the lightweight read models of type <typeparamref name="TLiteRow"/>, projected from the data of type <typeparamref name="TModel"/> stored in the storage medium.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter carrying the criteria used to filter the results.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching data is returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of type <typeparamref name="TLiteRow"/>, or an error.</returns>
    Task<Result<PaginatedResultDto<TLiteRow>>> GetAllLiteAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = false, bool shouldTrackEntities = false, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto;
}
