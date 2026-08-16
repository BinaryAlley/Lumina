#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Actions;

/// <summary>
/// Interface defining the "get paginated" action for interacting with a generic persistence medium.
/// </summary>
/// <typeparam name="TModel">The type used as a result for the "get paginated" action. It should implement <see cref="IStorageEntity"/>.</typeparam>
public interface IGetPaginatedRepositoryAction<TModel> where TModel : IStorageEntity
{
    /// <summary>
    /// Gets paginated data of type <typeparamref name="TModel"/> from the storage medium.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter carrying the criteria used to filter the results.</typeparam>
    /// <param name="paginationData">The pagination data that includes current page and number of items per page to retrieve. If <see langword="null"/>, all matching data is returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing parameters used to filter results.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of type <typeparamref name="TModel"/>, or an error.</returns>
    Task<Result<PaginatedResultDto<TModel>>> GetPaginatedAsync<TFilter>(PaginationDataDto? paginationData, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto;
}
