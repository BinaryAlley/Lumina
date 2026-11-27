#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;

/// <summary>
/// Validates the needed validation rules for <see cref="GetReadingSectionQuery"/>.
/// </summary>
public class GetReadingSectionQueryValidator : AbstractValidator<GetReadingSectionQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionQueryValidator"/> class.
    /// </summary>
    public GetReadingSectionQueryValidator()
    {
        // Validates the identifier of the media library the book belongs to, taken from the route.
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(query => query.LibraryId is not null && query.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the identifier of the book whose reading section is retrieved, taken from the route.
        RuleFor(query => query.BookId)
            .NotEmpty()
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);

        RuleFor(query => query.BookId)
            .Must(bookId => Guid.TryParse(bookId, out Guid parsedBookId) && parsedBookId != Guid.Empty)
            .When(query => query.BookId is not null && query.BookId.Length > 0)
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);

        // Validates the opaque location reference of the reading section, taken from the route.
        RuleFor(query => query.LocationRef)
            .NotEmpty()
            .WithError(Errors.Reading.LocationRefCannotBeEmpty);
    }
}
