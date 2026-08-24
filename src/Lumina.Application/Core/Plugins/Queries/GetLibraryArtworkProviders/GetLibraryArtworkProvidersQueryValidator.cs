#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryArtworkProviders;

/// <summary>
/// Validates the needed validation rules for <see cref="GetLibraryArtworkProvidersQuery"/>.
/// </summary>
public class GetLibraryArtworkProvidersQueryValidator : AbstractValidator<GetLibraryArtworkProvidersQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryArtworkProvidersQueryValidator"/> class.
    /// </summary>
    public GetLibraryArtworkProvidersQueryValidator()
    {
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }
}
