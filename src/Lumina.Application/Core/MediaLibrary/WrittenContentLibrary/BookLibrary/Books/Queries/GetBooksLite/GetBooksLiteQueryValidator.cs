#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;

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

        // the alpha key must be exactly one of the three picker bucket kinds, so that the filter specification can
        // derive the key of a title unambiguously: no filter, a number bucket, a symbol bucket, or a single ASCII letter
        RuleFor(query => query.Filter.FilterAlphaKey)
            .Must(alphaKey => alphaKey is null
                || alphaKey == LibraryItemAlphaKeys.NUMBER
                || alphaKey == LibraryItemAlphaKeys.SYMBOL
                || (alphaKey.Length == 1 && char.IsAsciiLetter(alphaKey[0])))
            .WithError(Errors.Library.InvalidFilterAlphaKey);
    }
}
