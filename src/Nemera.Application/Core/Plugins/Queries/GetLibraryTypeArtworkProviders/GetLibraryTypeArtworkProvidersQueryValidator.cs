#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
#endregion

namespace Nemera.Application.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;

/// <summary>
/// Validates the needed validation rules for <see cref="GetLibraryTypeArtworkProvidersQuery"/>.
/// </summary>
public class GetLibraryTypeArtworkProvidersQueryValidator : AbstractValidator<GetLibraryTypeArtworkProvidersQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeArtworkProvidersQueryValidator"/> class.
    /// </summary>
    public GetLibraryTypeArtworkProvidersQueryValidator()
    {
        RuleFor(query => query.LibraryType)
            .NotNull()
            .WithError(Errors.Library.LibraryTypeCannotBeNull)
            .IsInEnum<GetLibraryTypeArtworkProvidersQuery, LibraryType>()
            .WithError(Errors.Library.UnknownLibraryType);
    }
}
