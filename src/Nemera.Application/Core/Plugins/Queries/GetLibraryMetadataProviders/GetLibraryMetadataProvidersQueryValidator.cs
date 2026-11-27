#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;

/// <summary>
/// Validates the needed validation rules for <see cref="GetLibraryMetadataProvidersQuery"/>.
/// </summary>
public class GetLibraryMetadataProvidersQueryValidator : AbstractValidator<GetLibraryMetadataProvidersQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryMetadataProvidersQueryValidator"/> class.
    /// </summary>
    public GetLibraryMetadataProvidersQueryValidator()
    {
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }
}
