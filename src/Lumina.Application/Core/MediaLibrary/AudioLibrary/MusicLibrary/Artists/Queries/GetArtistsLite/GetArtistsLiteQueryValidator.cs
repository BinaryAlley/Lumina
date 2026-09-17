#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;

/// <summary>
/// Validates the needed validation rules for <see cref="GetArtistsLiteQuery"/>.
/// </summary>
public class GetArtistsLiteQueryValidator : AbstractValidator<GetArtistsLiteQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsLiteQueryValidator"/> class.
    /// </summary>
    public GetArtistsLiteQueryValidator()
    {
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty)
            .Must(libraryId => libraryId != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
