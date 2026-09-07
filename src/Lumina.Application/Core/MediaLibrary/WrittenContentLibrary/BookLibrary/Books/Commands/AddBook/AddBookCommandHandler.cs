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
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public AddBookCommandHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<AddBookCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
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

        // Admins can add books to all libraries; for everyone else, only to the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(command.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // TODO: update Api.Book.md documentation when the functionality is fully implemented.
        BookSeries? bookSeries = null;
        if (command.Series != null)
        {
            // TODO: add logic to search the book series repository for existing book series, based on the provided title.
            // TODO: uncomment integration and unit tests about series.
        }
        List<Result<BookRating>> domainRatingsResult = command.Ratings!.ConvertAll(rating => BookRating.Create(
                rating.Value ?? default,
                rating.MaxValue ?? default,
                Optional<BookRatingSource>.FromNullable(rating.Source.HasValue ? (BookRatingSource)(int)rating.Source : (BookRatingSource?)null),
                Optional<int>.FromNullable(rating.VoteCount)));
        // Check if any of the results contain errors.
        List<Error> errors = [.. domainRatingsResult.Where(ratingResult => ratingResult.IsFailure).SelectMany(ratingResult => ratingResult.Errors)];
        if (errors.Count != 0) // if there are errors, return them            
            return errors;

        List<BookRating> domainRatings = [.. domainRatingsResult.Select(rating => rating.Value)];

        List<Result<Genre>> domainGenresResult = command.Metadata!.Genres!.ConvertAll(genre => Genre.Create(genre.Name!));
        errors = [.. domainGenresResult.Where(genreResult => genreResult.IsFailure).SelectMany(genreResult => genreResult.Errors)];
        if (errors.Count != 0)
            return errors;
        List<Genre> domainGenres = [.. domainGenresResult.Select(genre => genre.Value)];

        List<Result<Tag>> domainTagsResult = command.Metadata.Tags!.ConvertAll(tag => Tag.Create(tag.Name!));
        errors = [.. domainTagsResult.Where(tagResult => tagResult.IsFailure).SelectMany(tagResult => tagResult.Errors)];
        if (errors.Count != 0)
            return errors;
        List<Tag> domainTags = [.. domainTagsResult.Select(tag => tag.Value)];

        List<Result<Isbn>> domainIsbnsResult = command.ISBNs!.ConvertAll(isbn => Isbn.Create(isbn.Value!, (IsbnFormat)(int)isbn.Format!));
        errors = [.. domainIsbnsResult.Where(isbnResult => isbnResult.IsFailure).SelectMany(isbnResult => isbnResult.Errors)];
        if (errors.Count != 0)
            return errors;
        List<Isbn> domainIsbns = [.. domainIsbnsResult.Select(isbn => isbn.Value)];

        Result<ReleaseInfo> releaseInfoResult = ReleaseInfo.Create(
            Optional<DateOnly>.FromNullable(command.Metadata.ReleaseInfo!.OriginalReleaseDate),
            Optional<int>.FromNullable(command.Metadata.ReleaseInfo.OriginalReleaseYear),
            Optional<DateOnly>.FromNullable(command.Metadata.ReleaseInfo.ReReleaseDate),
            Optional<int>.FromNullable(command.Metadata.ReleaseInfo.ReReleaseYear),
            Optional<ReleaseCountry>.FromNullable(command.Metadata.ReleaseInfo!.ReleaseCountry),
            Optional<string>.FromNullable(command.Metadata.ReleaseInfo.ReleaseVersion)
        );
        if (releaseInfoResult.IsFailure)
            return releaseInfoResult.Errors;
        ReleaseInfo releaseInfo = releaseInfoResult.Value;
        LanguageInfo? languageInfo = null;
        if (command.Metadata.Language is not null)
        {
            Result<LanguageInfo> languageInfoResult = LanguageInfo.Create(
                command.Metadata.Language.LanguageCode!,
                command.Metadata.Language.LanguageName!,
                Optional<string>.FromNullable(command.Metadata.Language.NativeName));
            if (languageInfoResult.IsFailure)
                return languageInfoResult.Errors;
            languageInfo = languageInfoResult.Value;
        }
        LanguageInfo? originalLanguageInfo = null;
        if (command.Metadata.OriginalLanguage is not null)
        {
            Result<LanguageInfo> originalLanguageInfoResult = LanguageInfo.Create(
                command.Metadata.OriginalLanguage.LanguageCode!,
                command.Metadata.OriginalLanguage.LanguageName!,
                Optional<string>.FromNullable(command.Metadata.OriginalLanguage.NativeName));
            if (originalLanguageInfoResult.IsFailure)
                return originalLanguageInfoResult.Errors;
            originalLanguageInfo = originalLanguageInfoResult.Value;
        }
        Result<WrittenContentMetadata> metadataResult = WrittenContentMetadata.Create(
            command.Metadata.Title!,
            Optional<string>.FromNullable(command.Metadata.OriginalTitle),
            Optional<string>.FromNullable(command.Metadata.Description),
            releaseInfo,
            domainGenres,
            domainTags,
            Optional<LanguageInfo>.FromNullable(languageInfo),
            Optional<LanguageInfo>.FromNullable(originalLanguageInfo),
            Optional<string>.FromNullable(command.Metadata.Publisher),
            Optional<int>.FromNullable(command.Metadata.PageCount)
        );
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

        // Link the media contributors typed by the user to the book, finding or creating a single contributor per person; the same person
        // can be credited under several roles, so every distinct display name must resolve to the same media contributor row.
        List<MediaContributorId> contributorIds = [];
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

            contributorIds.Add(MediaContributorId.Create(contributorEntity.Id));
        }

        Result<Book> createBookResult = Book.Create(
            LibraryId.Create(command.LibraryId),
            command.Path,
            metadataResult.Value,
            Optional<BookFormat>.FromNullable(command.Format),
            Optional<string>.FromNullable(command.Edition),
            command.VolumeNumber ?? default,
            Optional<BookSeries>.FromNullable(bookSeries),
            Optional<string>.FromNullable(command.ASIN),
            Optional<string>.FromNullable(command.GoodreadsId),
            Optional<string>.FromNullable(command.LCCN),
            Optional<string>.FromNullable(command.OCLCNumber),
            Optional<string>.FromNullable(command.OpenLibraryId),
            Optional<string>.FromNullable(command.LibraryThingId),
            Optional<string>.FromNullable(command.GoogleBooksId),
            Optional<string>.FromNullable(command.BarnesAndNobleId),
            Optional<string>.FromNullable(command.AppleBooksId),
            domainIsbns,
            contributorIds,
            ratings: domainRatings
        );
        if (createBookResult.IsFailure)
            return createBookResult.Errors;

        // The participation rows need the generated id of the book, so they are built once the domain book exists.
        BookEntity persistenceBook = createBookResult.Value.ToRepositoryEntity();
        persistenceBook.CreatedBy = userId;
        persistenceBook.BookContributors = [.. command.Contributors!
            .Where(contributor => contributor.Name?.DisplayName is not null)
            .Select(contributor => new BookContributorEntity
            {
                Id = Guid.NewGuid(),
                BookId = persistenceBook.Id,
                MediaContributorId = contributorsByDisplayName[contributor.Name!.DisplayName!].Id,
                RoleName = contributor.Role?.Name ?? "Contributor",
                RoleCategory = contributor.Role?.Category ?? MediaContributorRoleCategory.Other,
                CreatedOnUtc = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = null
            })];
        Result<Created> insertBookResult = await _unitOfWork.BookRepository.InsertAsync(persistenceBook, cancellationToken).ConfigureAwait(false);
        if (insertBookResult.IsFailure)
            return insertBookResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return persistenceBook.ToResponse() with { Contributors = command.Contributors };
    }
}
