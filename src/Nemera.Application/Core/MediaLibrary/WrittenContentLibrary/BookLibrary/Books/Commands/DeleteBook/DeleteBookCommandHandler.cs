#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;

/// <summary>
/// Handler for the command to delete a book by its Id.
/// </summary>
public class DeleteBookCommandHandler : ICommandHandler<DeleteBookCommand, Result<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<DeleteBookCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public DeleteBookCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IValidator<DeleteBookCommand> validator)
    {
        _unitOfWork = unitOfWork;
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

        // The book is soft deleted, so its metadata, artwork and path stay available until the user restores it or the file disappears from disk.
        // Keeping the row is also what makes the deletion survive an automatic scan, because the scan treats its path as already catalogued.
        Result<Updated> deleteBookResult = await _unitOfWork.BookRepository.SoftDeleteByIdAsync(existingBook.Id, cancellationToken).ConfigureAwait(false);
        if (deleteBookResult.IsFailure)
            return deleteBookResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return Result.Deleted;
    }
}
