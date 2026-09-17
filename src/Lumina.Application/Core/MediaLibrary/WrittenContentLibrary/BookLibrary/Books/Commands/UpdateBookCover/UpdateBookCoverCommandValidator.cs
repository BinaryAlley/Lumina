#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdateBookCoverCommand"/>.
/// </summary>
public class UpdateBookCoverCommandValidator : AbstractValidator<UpdateBookCoverCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverCommandValidator"/> class.
    /// </summary>
    public UpdateBookCoverCommandValidator()
    {
        // Validates the identifier of the media library the book belongs to, taken from the route.
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(command => command.LibraryId is not null && command.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the identifier of the book whose cover image is updated, taken from the route.
        RuleFor(command => command.BookId)
            .NotEmpty()
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);

        RuleFor(command => command.BookId)
            .Must(bookId => Guid.TryParse(bookId, out Guid parsedBookId) && parsedBookId != Guid.Empty)
            .When(command => command.BookId is not null && command.BookId.Length > 0)
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);

        // Validates the uploaded cover image.
        RuleFor(command => command.Cover)
            .NotNull()
            .WithError(Errors.WrittenContent.BookCoverCannotBeNull);

        RuleFor(command => command.FileName)
            .NotEmpty()
            .When(command => command.Cover is not null)
            .WithError(Errors.FileSystemManagement.FileNameCannotBeEmpty);
    }
}
