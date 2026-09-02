#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;

/// <summary>
/// Handler for the query to get the content of a reading section of a book.
/// </summary>
public class GetReadingSectionQueryHandler : IQueryHandler<GetReadingSectionQuery, Result<ReadingSectionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBookReadingService _bookReadingService;
    private readonly IValidator<GetReadingSectionQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="bookReadingService">Injected service for reading books using the book reader plugins.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetReadingSectionQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IBookReadingService bookReadingService, IValidator<GetReadingSectionQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _bookReadingService = bookReadingService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the content of a reading section of a book.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the content of the reading section, or an error.
    /// </returns>
    public async Task<Result<ReadingSectionDto>> HandleAsync(GetReadingSectionQuery query, CancellationToken cancellationToken)
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

        // The per-user reading preferences decide how the book is rendered: whether a PDF is rendered as page images or has its text
        // layer extracted, and whether the styles of the section content are preserved; a missing settings row falls back to the
        // defaults, which are to extract the text layer and to preserve the styles.
        Result<UserSettingsEntity?> getSettingsResult = await _unitOfWork.UserSettingsRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getSettingsResult.IsFailure)
            return getSettingsResult.Errors;
        bool shouldRenderPdfAsImages = getSettingsResult.Value is not null && getSettingsResult.Value.ShouldRenderPdfAsImages;
        bool shouldPreserveStyles = getSettingsResult.Value is null || getSettingsResult.Value.ShouldPreserveBookStyles;

        return await _bookReadingService.GetSectionAsync(book.Id, book.LibraryId, book.Path, library.LibraryType, query.LocationRef, shouldRenderPdfAsImages, shouldPreserveStyles, cancellationToken).ConfigureAwait(false);
    }
}
