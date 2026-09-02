#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;

/// <summary>
/// Validates the needed validation rules for <see cref="GetReadingResourceQuery"/>.
/// </summary>
public class GetReadingResourceQueryValidator : AbstractValidator<GetReadingResourceQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceQueryValidator"/> class.
    /// </summary>
    public GetReadingResourceQueryValidator()
    {
        RuleFor(query => query.BookId)
            .NotEmpty()
            .WithError(Errors.Reading.BookIdCannotBeEmpty);

        RuleFor(query => query.ResourceKey)
            .NotEmpty()
            .WithError(Errors.Reading.ResourceKeyCannotBeEmpty);
    }
}
