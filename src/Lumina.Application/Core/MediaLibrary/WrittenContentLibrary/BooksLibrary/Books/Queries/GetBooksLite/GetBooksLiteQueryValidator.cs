#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooksLite;

/// <summary>
/// Validates the needed validation rules for <see cref="GetBooksLiteQuery"/>.
/// </summary>
public class GetBooksLiteQueryValidator : AbstractValidator<GetBooksLiteQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksLiteQueryValidator"/> class.
    /// </summary>
    public GetBooksLiteQueryValidator()
    {
        RuleFor(query => query.Filter.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
