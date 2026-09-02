#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;

/// <summary>
/// Validates the needed validation rules for <see cref="GetReadingSectionQuery"/>.
/// </summary>
public class GetReadingSectionQueryValidator : AbstractValidator<GetReadingSectionQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionQueryValidator"/> class.
    /// </summary>
    public GetReadingSectionQueryValidator()
    {
        RuleFor(query => query.BookId)
            .NotEmpty()
            .WithError(Errors.Reading.BookIdCannotBeEmpty);

        RuleFor(query => query.LocationRef)
            .NotEmpty()
            .WithError(Errors.Reading.LocationRefCannotBeEmpty);
    }
}
