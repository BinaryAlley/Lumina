#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Contracts.Models.MediaContributors;
using Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.Aggregates.MediaContributor.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using MapsterMapper;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;

/// <summary>
/// Handler for the command to add a book.
/// </summary>
public class AddBookCommandHandler : IRequestHandler<AddBookCommand, ErrorOr<Book>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="mapper">Injected service for mapping objects.</param>
    public AddBookCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the command to add a book.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="Book"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<Book>> Handle(AddBookCommand request, CancellationToken cancellationToken)
    {
        // TODO: update Api.Book.md documentation when the functionality is fully implemented
        List<MediaContributorId> contributorIds = [];
        foreach (MediaContributorModel mediaContributor in request.Contributors)
        {
            // TODO: add logic to search the media contributors repository for existing contributors, based on the provided names
        }
        BookSeries? bookSeries = null;
        if (request.Series != null)
        {
            // TODO: add logic to search the book series repository for existing book series, based on the provided title
            // TODO: uncomment integration and unit tests about series
        }
        List<ErrorOr<BookRating>> domainRatingsResult = request.Ratings.ConvertAll(rating => BookRating.Create(
                rating.Value ?? default,
                rating.MaxValue ?? default,
                Optional<BookRatingSource>.FromNullable(rating.Source.HasValue ? (BookRatingSource)(int)rating.Source : default),
                Optional<int>.FromNullable(rating.VoteCount)));
        // check if any of the results contain errors
        List<Error> errors = domainRatingsResult.Where(ratingResult => ratingResult.IsError).SelectMany(ratingResult => ratingResult.Errors).ToList();
        if (errors.Count != 0) // if there are errors, return them            
            return errors;

        List<BookRating> domainRatings = domainRatingsResult.Select(rating => rating.Value).ToList();

        List<ErrorOr<Genre>> domainGenresResult = request.Metadata.Genres!.ConvertAll(genre => Genre.Create(genre.Name!));
        errors = domainGenresResult.Where(genreResult => genreResult.IsError).SelectMany(genreResult => genreResult.Errors).ToList();
        if (errors.Count != 0)
            return errors;
        List<Genre> domainGenres = domainGenresResult.Select(genre => genre.Value).ToList();

        List<ErrorOr<Tag>> domainTagsResult = request.Metadata.Tags!.ConvertAll(tag => Tag.Create(tag.Name!));
        errors = domainTagsResult.Where(tagResult => tagResult.IsError).SelectMany(tagResult => tagResult.Errors).ToList();
        if (errors.Count != 0)
            return errors;
        List<Tag> domainTags = domainTagsResult.Select(tag => tag.Value).ToList();

        List<ErrorOr<Isbn>> domainIsbnsResult = request.ISBNs.ConvertAll(isbn => Isbn.Create(isbn.Value!, (IsbnFormat)(int)isbn.Format!));
        errors = domainIsbnsResult.Where(isbnResult => isbnResult.IsError).SelectMany(isbnResult => isbnResult.Errors).ToList();
        if (errors.Count != 0)
            return errors;
        List<Isbn> domainIsbns = domainIsbnsResult.Select(isbn => isbn.Value).ToList();

        ErrorOr<ReleaseInfo> releaseInfoResult = ReleaseInfo.Create(
            Optional<DateOnly>.FromNullable(request.Metadata.ReleaseInfo!.OriginalReleaseDate),
            Optional<int>.FromNullable(request.Metadata.ReleaseInfo.OriginalReleaseYear),
            Optional<DateOnly>.FromNullable(request.Metadata.ReleaseInfo.ReReleaseDate),
            Optional<int>.FromNullable(request.Metadata.ReleaseInfo.ReReleaseYear),
            Optional<string>.FromNullable(request.Metadata.ReleaseInfo.ReleaseCountry),
            Optional<string>.FromNullable(request.Metadata.ReleaseInfo.ReleaseVersion)
        );
        if (releaseInfoResult.IsError)
            return releaseInfoResult.Errors;
        ReleaseInfo releaseInfo = releaseInfoResult.Value;
        LanguageInfo? languageInfo = null;
        if (request.Metadata.Language is not null)
        {
            ErrorOr<LanguageInfo> languageInfoResult = LanguageInfo.Create(
                request.Metadata.Language.LanguageCode!,
                request.Metadata.Language.LanguageName!,
                Optional<string>.FromNullable(request.Metadata.Language.NativeName));
            if (languageInfoResult.IsError)
                return languageInfoResult.Errors;
            languageInfo = languageInfoResult.Value;
        }
        LanguageInfo? originalLanguageInfo = null;
        if (request.Metadata.OriginalLanguage is not null)
        {
            ErrorOr<LanguageInfo> originalLanguageInfoResult = LanguageInfo.Create(
                request.Metadata.OriginalLanguage.LanguageCode!,
                request.Metadata.OriginalLanguage.LanguageName!,
                Optional<string>.FromNullable(request.Metadata.OriginalLanguage.NativeName));
            if (originalLanguageInfoResult.IsError)
                return originalLanguageInfoResult.Errors;
            originalLanguageInfo = originalLanguageInfoResult.Value;
        }
        ErrorOr<WrittenContentMetadata> metadataResult = WrittenContentMetadata.Create(
            request.Metadata.Title!,
            Optional<string>.FromNullable(request.Metadata.OriginalTitle),
            Optional<string>.FromNullable(request.Metadata.Description),
            releaseInfo,
            domainGenres,
            domainTags,
            Optional<LanguageInfo>.FromNullable(languageInfo),
            Optional<LanguageInfo>.FromNullable(originalLanguageInfo),
            Optional<string>.FromNullable(request.Metadata.Publisher),
            Optional<int>.FromNullable(request.Metadata.PageCount)
        );
        if (metadataResult.IsError)
            return metadataResult.Errors;
        ErrorOr<Book> createBookResult = Book.Create(
            metadataResult.Value,
            Optional<BookFormat>.FromNullable(request.Format),
            Optional<string>.FromNullable(request.Edition),
            request.VolumeNumber ?? default,
            Optional<BookSeries>.FromNullable(bookSeries),
            Optional<string>.FromNullable(request.ASIN),
            Optional<string>.FromNullable(request.GoodreadsId),
            Optional<string>.FromNullable(request.LCCN),
            Optional<string>.FromNullable(request.OCLCNumber),
            Optional<string>.FromNullable(request.OpenLibraryId),
            Optional<string>.FromNullable(request.LibraryThingId),
            Optional<string>.FromNullable(request.GoogleBooksId),
            Optional<string>.FromNullable(request.BarnesAndNobleId),
            Optional<string>.FromNullable(request.AppleBooksId),
            domainIsbns,
            contributorIds,
            ratings: domainRatings
        );
        if (createBookResult.IsError)
            return createBookResult.Errors;

        IBookRepository bookRepository = _unitOfWork.GetRepository<IBookRepository>();
        BookModel persistenceBook = _mapper.Map<BookModel>(createBookResult.Value);
        ErrorOr<Created> insertBookResult = await bookRepository.InsertAsync(persistenceBook, cancellationToken).ConfigureAwait(false);
        if (insertBookResult.IsError)
            return insertBookResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return createBookResult.Value;
    }
}
