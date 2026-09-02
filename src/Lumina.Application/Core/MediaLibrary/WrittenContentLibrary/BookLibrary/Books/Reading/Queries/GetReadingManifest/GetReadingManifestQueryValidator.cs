#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;

/// <summary>
/// Validates the needed validation rules for <see cref="GetReadingManifestQuery"/>.
/// </summary>
public class GetReadingManifestQueryValidator : AbstractValidator<GetReadingManifestQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingManifestQueryValidator"/> class.
    /// </summary>
    public GetReadingManifestQueryValidator()
    {
        RuleFor(query => query.BookId)
            .NotEmpty()
            .WithError(Errors.Reading.BookIdCannotBeEmpty);
    }
}
