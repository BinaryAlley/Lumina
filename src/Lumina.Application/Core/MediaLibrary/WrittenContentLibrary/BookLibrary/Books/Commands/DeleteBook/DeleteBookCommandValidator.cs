#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;

/// <summary>
/// Validates the needed validation rules for <see cref="DeleteBookCommand"/>.
/// </summary>
public class DeleteBookCommandValidator : AbstractValidator<DeleteBookCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookCommandValidator"/> class.
    /// </summary>
    public DeleteBookCommandValidator()
    {
        // Validates the identifier of the media library that owns the book, taken from the route.
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(command => command.LibraryId is not null && command.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the identifier of the book, taken from the route.
        RuleFor(command => command.BookId)
            .NotEmpty()
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);

        RuleFor(command => command.BookId)
            .Must(bookId => Guid.TryParse(bookId, out Guid parsedBookId) && parsedBookId != Guid.Empty)
            .When(command => command.BookId is not null && command.BookId.Length > 0)
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }
}
