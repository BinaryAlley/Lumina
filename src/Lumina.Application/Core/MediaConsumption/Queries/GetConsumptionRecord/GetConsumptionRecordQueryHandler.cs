#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionRecord;

/// <summary>
/// Handler for the query to get a consumption record of the current user.
/// </summary>
public class GetConsumptionRecordQueryHandler : IQueryHandler<GetConsumptionRecordQuery, Result<ConsumptionRecordResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetConsumptionRecordQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionRecordQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetConsumptionRecordQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IValidator<GetConsumptionRecordQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get a consumption record of the current user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the requested <see cref="ConsumptionRecordResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<ConsumptionRecordResponse>> HandleAsync(GetConsumptionRecordQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifier is a non-empty Guid before this point.
        Guid consumptionRecordId = Guid.Parse(query.ConsumptionRecordId!);

        // The record is read without tracking and scoped to the current user, so a record of another user is indistinguishable from a missing one.
        Result<MediaConsumptionRecordEntity?> getRecordResult = await _unitOfWork.MediaConsumptionRecordRepository
            .GetByIdForUserAsync(consumptionRecordId, userId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getRecordResult.IsFailure)
            return getRecordResult.Errors;
        if (getRecordResult.Value is null)
            return DomainErrors.MediaConsumption.ConsumptionRecordNotFound;

        return getRecordResult.Value.ToDto().ToResponse();
    }
}
