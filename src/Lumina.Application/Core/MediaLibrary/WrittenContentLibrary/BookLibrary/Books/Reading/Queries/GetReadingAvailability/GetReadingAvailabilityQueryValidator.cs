#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;

/// <summary>
/// Validates the needed validation rules for <see cref="GetReadingAvailabilityQuery"/>.
/// </summary>
public class GetReadingAvailabilityQueryValidator : AbstractValidator<GetReadingAvailabilityQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingAvailabilityQueryValidator"/> class.
    /// </summary>
    public GetReadingAvailabilityQueryValidator()
    {
        RuleFor(query => query.BookId)
            .NotEmpty()
            .WithError(Errors.Reading.BookIdCannotBeEmpty);
    }
}
