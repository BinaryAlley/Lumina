#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
#endregion

namespace Nemera.Application.Core.Plugins.Queries.GetLibraryTypeMetadataProviders;

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
