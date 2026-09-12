#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;

/// <summary>
/// Handler for the command to update an existing book.
/// </summary>
public class UpdateBookCommandHandler : ICommandHandler<UpdateBookCommand, Result<BookResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<UpdateBookCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdateBookCommandHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<UpdateBookCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update an existing book.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the updated <see cref="BookResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<BookResponse>> HandleAsync(UpdateBookCommand command, CancellationToken cancellationToken)
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

        // Get the existing book with all its related data.
        Result<BookEntity?> getBookResult = await _unitOfWork.BookRepository.GetByIdAsync(bookId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getBookResult.IsFailure)
            return getBookResult.Errors;
        if (getBookResult.Value is null)
            return DomainErrors.WrittenContent.BookNotFound;
        BookEntity existingBook = getBookResult.Value;

        // Resource scoping: the book must belong to the library named by the route, so that a book can never be edited through another
        // library's route; the mismatch is reported as not found, without disclosing that the book exists in another library.
        if (existingBook.LibraryId != libraryId)
            return DomainErrors.WrittenContent.BookNotFound;

        // Admins can update the books of all libraries; for everyone else, only the books of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingBook.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // Media contributors are referenced by Id and are never created implicitly by editing a book, so each one must already exist.
        Result<IReadOnlyList<MediaContributorEntity>> getContributorsResult = await GetExistingContributorsAsync(command, cancellationToken).ConfigureAwait(false);
        if (getContributorsResult.IsFailure)
            return getContributorsResult.Errors;

        // Convert the command to a domain aggregate to enforce invariants, preserving the identity and creation metadata of the stored book.
        Result<Book> updateBookResult = command.ToDomainEntity(existingBook);
        if (updateBookResult.IsFailure)
            return updateBookResult.Errors;

        // Map the updated domain book onto a fresh repository entity, ready for the repository to replace the stored data.
        BookEntity updatedBook = updateBookResult.Value.ToRepositoryEntity();

        // The enrichment and artwork columns are never overwritten by an edit, so they are copied onto the fresh entity that the repository
        // replaces the stored data with; without this, the stored artwork would be cleared.
        updatedBook.MetadataStatus = existingBook.MetadataStatus;
        updatedBook.LastMetadataUpdateUtc = existingBook.LastMetadataUpdateUtc;
        updatedBook.MetadataProvider = existingBook.MetadataProvider;
        updatedBook.BookArtwork = existingBook.BookArtwork;

        Result<Updated> updateResult = await _unitOfWork.BookRepository.UpdateAsync(updatedBook, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Returns the book as it was actually persisted, instead of the in-memory aggregate, so the response reflects every value the persistence medium applied on save.
        Result<BookEntity?> getPersistedBookResult = await _unitOfWork.BookRepository.GetByIdAsync(existingBook.Id, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPersistedBookResult.IsFailure)
            return getPersistedBookResult.Errors;
        if (getPersistedBookResult.Value is null)
            return DomainErrors.WrittenContent.BookNotFound;

        return getPersistedBookResult.Value.ToResponse();
    }

    /// <summary>
    /// Gets the media contributors referenced by <paramref name="command"/>, requiring each referenced Id to already exist.
    /// </summary>
    /// <param name="command">The command carrying the referenced media contributors.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the existing media contributors, or an error when at least one referenced contributor does not exist.
    /// </returns>
    private async Task<Result<IReadOnlyList<MediaContributorEntity>>> GetExistingContributorsAsync(UpdateBookCommand command, CancellationToken cancellationToken)
    {
        List<Guid> contributorIds = [.. command.Contributors!.Select(contributor => contributor.ContributorId).Distinct()];
        if (contributorIds.Count == 0)
            return Result.From<IReadOnlyList<MediaContributorEntity>>([]);

        Result<IReadOnlyList<MediaContributorEntity>> getContributorsResult = await _unitOfWork.MediaContributorRepository
            .GetByIdsAsync(contributorIds, cancellationToken).ConfigureAwait(false);
        if (getContributorsResult.IsFailure)
            return getContributorsResult.Errors;
        if (getContributorsResult.Value.Count != contributorIds.Count)
            return DomainErrors.MediaContributor.MediaContributorNotFound;
        return Result.From(getContributorsResult.Value);
    }
}
