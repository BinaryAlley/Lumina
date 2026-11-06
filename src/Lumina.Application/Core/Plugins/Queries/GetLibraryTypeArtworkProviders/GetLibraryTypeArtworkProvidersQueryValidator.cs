#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;

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
