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
        RuleFor(command => command.BookId)
            .NotEmpty()
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);

        RuleFor(command => command.Cover)
            .NotNull()
            .WithError(Errors.WrittenContent.BookCoverCannotBeNull);
    }
}
