#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;

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
