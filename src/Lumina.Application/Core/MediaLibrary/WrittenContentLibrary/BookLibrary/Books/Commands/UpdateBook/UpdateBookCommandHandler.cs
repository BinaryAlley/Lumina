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
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
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

        // Get the existing book with all its related data.
        Result<BookEntity?> getBookResult = await _unitOfWork.BookRepository.GetByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (getBookResult.IsFailure)
            return getBookResult.Errors;
        if (getBookResult.Value is null)
            return DomainErrors.WrittenContent.BookNotFound;
        BookEntity existingBook = getBookResult.Value;

        // Admins can update the books of all libraries; for everyone else, only the books of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingBook.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // Convert the book to a domain object and apply the edited metadata to it.
        Result<Book> getDomainBookResult = existingBook.ToDomainEntity();
        if (getDomainBookResult.IsFailure)
            return getDomainBookResult.Errors;
        Book book = getDomainBookResult.Value;

        Result<Success> applyMetadataResult = book.ApplyMetadata(command.ToBookMetadataDto());
        if (applyMetadataResult.IsFailure)
            return applyMetadataResult.Errors;

        // Link the media contributors typed by the user to the book, finding or creating a single contributor per person.
        List<BookContributorEntity> linkedContributors = [];
        // A person can be credited under several roles in the same book; each distinct display name must resolve to the same media
        // contributor row, otherwise the duplicate inserts would violate the unique display name constraint when the changes are saved.
        Dictionary<string, MediaContributorEntity> contributorsByDisplayName = [];
        foreach (MediaContributorDto contributor in command.Contributors!)
        {
            if (contributor.Name?.DisplayName is null)
                continue;

            if (!contributorsByDisplayName.TryGetValue(contributor.Name.DisplayName, out MediaContributorEntity? contributorEntity))
            {
                Result<MediaContributorEntity> findOrCreateResult = await _unitOfWork.MediaContributorRepository.FindOrCreateByDisplayNameAsync(contributor.Name.DisplayName, contributor.Name.LegalName, cancellationToken).ConfigureAwait(false);
                if (findOrCreateResult.IsFailure)
                    return findOrCreateResult.Errors;
                contributorEntity = findOrCreateResult.Value;
                contributorsByDisplayName.Add(contributor.Name.DisplayName, contributorEntity);
            }

            string roleName = contributor.Role?.Name ?? "Contributor";
            MediaContributorRoleCategory roleCategory = contributor.Role?.Category ?? MediaContributorRoleCategory.Other;
            linkedContributors.Add(new BookContributorEntity
            {
                Id = Guid.NewGuid(),
                BookId = existingBook.Id,
                MediaContributorId = contributorEntity.Id,
                RoleName = roleName,
                RoleCategory = roleCategory,
                CreatedOnUtc = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = null
            });
        }
        book.UpdateContributors([.. linkedContributors.Select(linkedContributor => MediaContributorId.Create(linkedContributor.MediaContributorId))]);

        // Map the updated domain book onto a fresh repository entity, ready for the repository to replace the stored data.
        BookEntity updatedBook = book.ToRepositoryEntity();
        updatedBook.BookContributors = linkedContributors;
        updatedBook.UpdatedOnUtc = DateTime.UtcNow;
        updatedBook.UpdatedBy = userId;

        // The identity, enrichment and artwork columns are never overwritten by an edit, so they are copied onto the fresh entity that
        // backs the response; without this, the response would report the fresh entity defaults (Pending metadata status, no cover path).
        updatedBook.CreatedOnUtc = existingBook.CreatedOnUtc;
        updatedBook.MetadataStatus = existingBook.MetadataStatus;
        updatedBook.LastMetadataUpdateUtc = existingBook.LastMetadataUpdateUtc;
        updatedBook.MetadataProvider = existingBook.MetadataProvider;
        updatedBook.BookArtwork = existingBook.BookArtwork;

        Result<Updated> updateResult = await _unitOfWork.BookRepository.UpdateAsync(updatedBook, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult.Errors;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return updatedBook.ToResponse() with { Contributors = command.Contributors };
    }
}
