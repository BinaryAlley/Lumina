#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;

/// <summary>
/// Validates the needed validation rules for <see cref="GetBookQuery"/>.
/// </summary>
public class GetBookQueryValidator : AbstractValidator<GetBookQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookQueryValidator"/> class.
    /// </summary>
    public GetBookQueryValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty()
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }
}
