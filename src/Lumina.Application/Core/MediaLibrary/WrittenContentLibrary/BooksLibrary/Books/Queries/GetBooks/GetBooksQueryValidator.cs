#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;

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
        RuleFor(query => query.Filter.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
