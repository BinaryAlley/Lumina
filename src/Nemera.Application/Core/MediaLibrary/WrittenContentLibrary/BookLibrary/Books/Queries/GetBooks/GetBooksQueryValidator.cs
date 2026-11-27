#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooks;

/// <summary>
/// Validates the needed validation rules for <see cref="GetBooksQuery"/>.
/// </summary>
public class GetBooksQueryValidator : AbstractValidator<GetBooksQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksQueryValidator"/> class.
    /// </summary>
    public GetBooksQueryValidator()
    {
        // Validates the identifier of the media library whose books are retrieved, taken from the route.
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(query => query.LibraryId is not null && query.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
