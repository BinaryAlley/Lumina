#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.RestoreBook;

/// <summary>
/// Validates the needed validation rules for <see cref="RestoreBookCommand"/>.
/// </summary>
public class RestoreBookCommandValidator : AbstractValidator<RestoreBookCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreBookCommandValidator"/> class.
    /// </summary>
    public RestoreBookCommandValidator()
    {
        // Validates the identifier of the media library that owns the book, taken from the route.
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(command => command.LibraryId is not null && command.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the identifier of the deleted book, taken from the route.
        RuleFor(command => command.BookId)
            .NotEmpty()
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);

        RuleFor(command => command.BookId)
            .Must(bookId => Guid.TryParse(bookId, out Guid parsedBookId) && parsedBookId != Guid.Empty)
            .When(command => command.BookId is not null && command.BookId.Length > 0)
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }
}
