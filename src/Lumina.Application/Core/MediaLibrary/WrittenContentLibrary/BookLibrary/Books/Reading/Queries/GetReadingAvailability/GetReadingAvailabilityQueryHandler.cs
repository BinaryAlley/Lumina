#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;

/// <summary>
/// Handler for the query to check the reading availability of a book.
/// </summary>
public class GetReadingAvailabilityQueryHandler : IQueryHandler<GetReadingAvailabilityQuery, Result<ReadingAvailabilityResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBookReadingService _bookReadingService;
    private readonly IValidator<GetReadingAvailabilityQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingAvailabilityQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="bookReadingService">Injected service for reading books using the book reader plugins.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetReadingAvailabilityQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IBookReadingService bookReadingService, IValidator<GetReadingAvailabilityQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _bookReadingService = bookReadingService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to check the reading availability of a book.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the reading availability of the book, or an error.
    /// </returns>
    public async Task<Result<ReadingAvailabilityResponse>> HandleAsync(GetReadingAvailabilityQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The authorization policy works on the media library, so the book is fetched first to learn its LibraryId, and
        // the library is fetched afterwards for its type, which the reader plugins match against when resolving the format.
        Result<BookEntity?> getBookResult = await _unitOfWork.BookRepository.GetByIdAsync(query.BookId, cancellationToken).ConfigureAwait(false);
        if (getBookResult.IsFailure)
            return getBookResult.Errors;
        if (getBookResult.Value is null)
            return DomainErrors.Reading.BookNotFound;
        BookEntity book = getBookResult.Value;

        // Admins can read the books of any library; for everyone else, only their own libraries.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(book.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(book.LibraryId, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;
        LibraryEntity library = getLibraryResult.Value;

        return await _bookReadingService.GetAvailabilityAsync(book.Id, book.LibraryId, book.Path, library.LibraryType, cancellationToken).ConfigureAwait(false);
    }
}
