#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.Plugins.Queries.GetLibraryArtworkProviders;

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
