#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;

/// <summary>
/// Handler for the command to update the cover image of an existing book.
/// </summary>
public class UpdateBookCoverCommandHandler : ICommandHandler<UpdateBookCoverCommand, Result<UpdateBookCoverResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookArtworkService _bookArtworkService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<UpdateBookCoverCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="bookArtworkService">Injected service for storing the artwork of a book.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdateBookCoverCommandHandler(IUnitOfWork unitOfWork, IBookArtworkService bookArtworkService, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<UpdateBookCoverCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _bookArtworkService = bookArtworkService;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update the cover image of an existing book.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the response carrying the relative path of the stored cover image, or an error message.
    /// </returns>
    public async Task<Result<UpdateBookCoverResponse>> HandleAsync(UpdateBookCoverCommand command, CancellationToken cancellationToken)
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

        // Get the existing book with its artwork.
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

        // The artwork is stored under the library and author segments, so their names are resolved to keep the location consistent with the scanned artwork.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(existingBook.LibraryId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;
        string libraryName = getLibraryResult.Value.Title;
        Result<string> authorNameResult = await GetAuthorNameAsync(existingBook, cancellationToken).ConfigureAwait(false);
        if (authorNameResult.IsFailure)
            return authorNameResult.Errors;
        string authorName = authorNameResult.Value;

        Result<string> storeArtworkResult = await _bookArtworkService.SaveBookArtworkAsync(
            existingBook.LibraryId,
            existingBook.Id,
            libraryName,
            authorName,
            existingBook.Title,
            command.Cover!,
            command.FileName!,
            cancellationToken).ConfigureAwait(false);
        if (storeArtworkResult.IsFailure)
            return storeArtworkResult.Errors;

        // Map the newly stored cover image onto the artwork of the book, replacing the existing cover or adding a new one.
        BookArtworkEntity? existingCover = existingBook.Artwork.FirstOrDefault(artwork => artwork.ArtworkType == ArtworkType.Cover);
        BookArtworkEntity coverArtwork = command.ToRepositoryEntity(existingCover, bookId, storeArtworkResult.Value, userId);
        if (existingCover is null)
            existingBook.Artwork.Add(coverArtwork);

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return new UpdateBookCoverResponse(storeArtworkResult.Value);
    }

    /// <summary>
    /// Gets the display name of the author of the <paramref name="book"/>, used to build the artwork storage path, or an empty string when the book has no author.
    /// </summary>
    /// <param name="book">The book whose author is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the display name of the author of the book, or an error when the referenced media contributors could not be retrieved.
    /// </returns>
    private async Task<Result<string>> GetAuthorNameAsync(BookEntity book, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> authorIds = [.. book.Contributors
            .Where(participation => participation.Role == MediaContributorRole.Author)
            .Select(participation => participation.MediaContributorId)
            .Distinct()];
        if (authorIds.Count == 0)
            return Result.From(string.Empty);

        Result<IReadOnlyList<MediaContributorEntity>> getContributorsResult = await _unitOfWork.MediaContributorRepository.GetByIdsAsync(authorIds, cancellationToken).ConfigureAwait(false);
        if (getContributorsResult.IsFailure)
            return getContributorsResult.Errors;
        return Result.From(getContributorsResult.Value.Select(contributor => contributor.DisplayName).FirstOrDefault() ?? string.Empty);
    }
}
