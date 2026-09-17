#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtists;

/// <summary>
/// Validates the needed validation rules for <see cref="GetArtistsQuery"/>.
/// </summary>
public class GetArtistsQueryValidator : AbstractValidator<GetArtistsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsQueryValidator"/> class.
    /// </summary>
    public GetArtistsQueryValidator()
    {
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty)
            .Must(libraryId => libraryId != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
