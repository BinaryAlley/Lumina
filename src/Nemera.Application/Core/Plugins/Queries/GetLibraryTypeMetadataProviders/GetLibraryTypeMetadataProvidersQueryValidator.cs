#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryTypeMetadataProviders;

/// <summary>
/// Validates the needed validation rules for <see cref="GetLibraryTypeMetadataProvidersQuery"/>.
/// </summary>
public class GetLibraryTypeMetadataProvidersQueryValidator : AbstractValidator<GetLibraryTypeMetadataProvidersQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeMetadataProvidersQueryValidator"/> class.
    /// </summary>
    public GetLibraryTypeMetadataProvidersQueryValidator()
    {
        RuleFor(query => query.LibraryType)
            .NotNull()
            .WithError(Errors.Library.LibraryTypeCannotBeNull)
            .IsInEnum<GetLibraryTypeMetadataProvidersQuery, LibraryType>()
            .WithError(Errors.Library.UnknownLibraryType);
    }
}
