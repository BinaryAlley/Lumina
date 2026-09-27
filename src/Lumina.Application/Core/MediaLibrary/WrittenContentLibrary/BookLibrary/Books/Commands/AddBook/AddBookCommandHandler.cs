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
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;

/// <summary>
/// Handler for the command to add a book.
/// </summary>
public class AddBookCommandHandler : ICommandHandler<AddBookCommand, Result<BookResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<AddBookCommand> _validator;
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    /// <param name="pathService">Injected service for file system path related functionality.</param>
    public AddBookCommandHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<AddBookCommand> validator, IPathService pathService)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
        _pathService = pathService;
    }

    /// <summary>
    /// Handles the command to add a book.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="BookResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<BookResponse>> HandleAsync(AddBookCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the library id of the route is a non-empty Guid before this point.
        Guid libraryId = Guid.Parse(command.LibraryId!);

        // Admins can add books to all libraries; for everyone else, only to the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // A book is only readable if it is stored inside one of the content locations of its library, so a client can never register an entry that points the reading pipeline at an arbitrary file of the host.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        if (!getLibraryResult.Value.ContentLocations.Any(contentLocation => _pathService.IsPathWithin(command.Path!, contentLocation.Path)))
            return DomainErrors.WrittenContent.BookPathMustBeWithinLibraryContentLocations;

        // Media contributors are referenced by Id and are never created implicitly by adding a book, so each one must already exist.
        Result<IReadOnlyList<MediaContributorEntity>> getContributorsResult = await GetExistingContributorsAsync(command, cancellationToken).ConfigureAwait(false);
        if (getContributorsResult.IsFailure)
            return getContributorsResult.Errors;

        // Convert the command to a domain aggregate to enforce invariants, then persist it in the repository.
        Result<Book> createBookResult = command.ToDomainEntity(libraryId);
        if (createBookResult.IsFailure)
            return createBookResult.Errors;

        Result<Created> insertBookResult = await _unitOfWork.BookRepository.InsertAsync(createBookResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (insertBookResult.IsFailure)
            return insertBookResult.Errors;

        // The pre-insert checks handle the common cases; a concurrent request could still persist a book at the same path of the same
        // library between them and the save, in which case the persistence medium reports the unique constraint violation as a conflict.
        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Returns the book as it was actually persisted, instead of the in-memory aggregate, so the response reflects every value the persistence medium applied on save.
        Result<BookEntity?> getPersistedBookResult = await _unitOfWork.BookRepository.GetByIdAsync(createBookResult.Value.Id.Value, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
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
    private async Task<Result<IReadOnlyList<MediaContributorEntity>>> GetExistingContributorsAsync(AddBookCommand command, CancellationToken cancellationToken)
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
