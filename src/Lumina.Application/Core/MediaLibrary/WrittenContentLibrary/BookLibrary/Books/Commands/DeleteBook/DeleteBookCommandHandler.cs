#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Domain.Common.Primitives;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;

/// <summary>
/// Handler for the command to delete a book by its Id.
/// </summary>
public class DeleteBookCommandHandler : ICommandHandler<DeleteBookCommand, Result<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookArtworkService _bookArtworkService;
    private readonly ILogger<DeleteBookCommandHandler> _logger;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<DeleteBookCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="bookArtworkService">Injected service for removing the artwork of a book.</param>
    /// <param name="logger">Injected logger used to report the issues encountered while removing the stored artwork.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public DeleteBookCommandHandler(
        IUnitOfWork unitOfWork,
        IBookArtworkService bookArtworkService,
        ILogger<DeleteBookCommandHandler> logger,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IValidator<DeleteBookCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _bookArtworkService = bookArtworkService;
        _logger = logger;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to delete a book by its Id.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> HandleAsync(DeleteBookCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifiers are non-empty Guids before this point.
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid bookId = Guid.Parse(command.BookId!);

        // Only the library of the book is needed to scope the delete, so the book row is read without its aggregate graph and without tracking.
        Result<BookEntity?> getBookResult = await _unitOfWork.BookRepository
            .GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getBookResult.IsFailure)
            return getBookResult.Errors;
        if (getBookResult.Value is null)
            return DomainErrors.WrittenContent.BookNotFound;
        BookEntity existingBook = getBookResult.Value;

        // Resource scoping: the book must belong to the library named by the route, so that a book can never be deleted through another
        // library's route; the mismatch is reported as not found, without disclosing that the book exists in another library.
        if (existingBook.LibraryId != libraryId)
            return DomainErrors.WrittenContent.BookNotFound;

        // Admins can delete the books of all libraries; for everyone else, only the books of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingBook.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // A book can only belong to a library that exists, so a client can never delete a book of a library of the host that is not there.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;
        LibraryEntity library = getLibraryResult.Value;

        // The artwork is stored under the library and author segments, so the author is resolved before the book is removed.
        Result<IReadOnlyDictionary<Guid, string?>> getAuthorsResult = await _unitOfWork.BookRepository
            .GetAuthorsDisplayNamesByBookIdsAsync([existingBook.Id], cancellationToken).ConfigureAwait(false);
        if (getAuthorsResult.IsFailure)
            return getAuthorsResult.Errors;
        string authorName = getAuthorsResult.Value.TryGetValue(existingBook.Id, out string? authorDisplayName) && authorDisplayName is not null ? authorDisplayName : string.Empty;

        // Delete the stored artwork of the book, best-effort, since a stale cover must not prevent the book from being removed.
        Result<Deleted> deleteArtworkResult = _bookArtworkService.DeleteBookArtwork(libraryId, existingBook.Id, library.Title, authorName, existingBook.Title);
        if (deleteArtworkResult.IsFailure)
            _logger.LogWarning("Failed to delete the stored artwork of the book with Id '{BookId}' at path '{BookPath}', the artwork might remain orphaned.", existingBook.Id, existingBook.Path);

        // Delete the book, whose stored artwork rows and participations are removed by the database cascade.
        Result<Deleted> deleteBookResult = await _unitOfWork.BookRepository.DeleteByIdAsync(existingBook.Id, cancellationToken).ConfigureAwait(false);
        if (deleteBookResult.IsFailure)
            return deleteBookResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return Result.Deleted;
    }
}
