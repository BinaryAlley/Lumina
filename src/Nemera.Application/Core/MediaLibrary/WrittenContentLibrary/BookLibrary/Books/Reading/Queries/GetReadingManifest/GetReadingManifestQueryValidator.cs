#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;

/// <summary>
/// Validates the needed validation rules for <see cref="GetReadingManifestQuery"/>.
/// </summary>
public class GetReadingManifestQueryValidator : AbstractValidator<GetReadingManifestQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingManifestQueryValidator"/> class.
    /// </summary>
    public GetReadingManifestQueryValidator()
    {
        // Validates the identifier of the media library the book belongs to, taken from the route.
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(query => query.LibraryId is not null && query.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the identifier of the book whose reading manifest is retrieved, taken from the route.
        RuleFor(query => query.BookId)
            .NotEmpty()
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);

        RuleFor(query => query.BookId)
            .Must(bookId => Guid.TryParse(bookId, out Guid parsedBookId) && parsedBookId != Guid.Empty)
            .When(query => query.BookId is not null && query.BookId.Length > 0)
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }
}
