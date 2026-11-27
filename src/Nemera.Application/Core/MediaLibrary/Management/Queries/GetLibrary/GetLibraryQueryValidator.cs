#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Queries.GetLibrary;

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
