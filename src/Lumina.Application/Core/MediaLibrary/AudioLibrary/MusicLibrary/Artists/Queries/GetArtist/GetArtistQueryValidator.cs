#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;

/// <summary>
/// Validates the needed validation rules for <see cref="GetArtistQuery"/>.
/// </summary>
public class GetArtistQueryValidator : AbstractValidator<GetArtistQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistQueryValidator"/> class.
    /// </summary>
    public GetArtistQueryValidator()
    {
        RuleFor(query => query.ArtistId)
            .NotEmpty()
            .WithError(Errors.Music.ArtistIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);
    }
}
