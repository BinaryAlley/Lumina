#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.Common;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="UpdateBookCommand"/>.
/// </summary>
public static class UpdateBookCommandMapping
{
    /// <summary>
    /// Converts <paramref name="command"/> to <see cref="Book"/>, preserving the identity and creation metadata of the stored book identified by <paramref name="existingBook"/>.
    /// </summary>
    /// <param name="command">The command to be converted.</param>
    /// <param name="existingBook">The stored book whose identity, file system path and creation metadata are preserved.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Book"/>, or an error message.
    /// </returns>
    public static Result<Book> ToDomainEntity(this UpdateBookCommand command, BookEntity existingBook)
    {
        IEnumerable<Result<BookRating>> domainRatingsResult = command.Ratings!.ToDomainEntities();
        List<Error> errors = [.. domainRatingsResult.Where(ratingResult => ratingResult.IsFailure).SelectMany(ratingResult => ratingResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<Isbn>> domainIsbnsResult = command.ISBNs!.ToDomainEntities();
        errors = [.. domainIsbnsResult.Where(isbnResult => isbnResult.IsFailure).SelectMany(isbnResult => isbnResult.Errors)];
        if (errors.Count > 0)
            return errors;

        Result<WrittenContentMetadata> metadataResult = command.Metadata!.ToDomainEntity();
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

        // TODO: update Api.Books.md documentation when the functionality is fully implemented.
        BookSeries? bookSeries = null;
        if (command.Series != null)
        {
            // TODO: add logic to search the book series repository for existing book series, based on the provided title.
            // TODO: uncomment integration and unit tests about series.
        }

        // Map the media contributors referenced by the user to their domain counterparts; a contributor is identified by its id and carries the role it played.
        IEnumerable<Result<BookMediaContributor>> domainContributorsResult = command.Contributors!.ToBookDomainEntities();
        errors = [.. domainContributorsResult.Where(domainContributor => domainContributor.IsFailure).SelectMany(domainContributor => domainContributor.Errors)];
        if (errors.Count > 0)
            return errors;

        return Book.Create(
            BookId.Create(existingBook.Id),
            LibraryId.Create(existingBook.LibraryId),
            existingBook.Path,
            metadataResult.Value,
            Optional<BookFormat>.FromNullable(command.Format),
            Optional<string>.FromNullable(command.Edition),
            Optional<float>.FromNullable(command.VolumeNumber),
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
            existingBook.CreatedOnUtc,
            Optional<DateTime>.FromNullable(existingBook.UpdatedOnUtc),
            [.. domainIsbnsResult.Select(isbn => isbn.Value)],
            [.. domainContributorsResult.Select(domainContributor => domainContributor.Value)],
            [.. domainRatingsResult.Select(domainRating => domainRating.Value)]
        );
    }
}
