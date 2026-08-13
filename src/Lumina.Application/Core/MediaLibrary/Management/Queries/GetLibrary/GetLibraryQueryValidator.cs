#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibrary;

/// <summary>
/// Validates the needed validation rules for <see cref="GetLibraryQuery"/>.
/// </summary>
public class GetLibraryQueryValidator : AbstractValidator<GetLibraryQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryQueryValidator"/> class.
    /// </summary>
    public GetLibraryQueryValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
