#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.Plugins.Queries.GetLibraryBookReaders;

/// <summary>
/// Validates the needed validation rules for <see cref="GetLibraryBookReadersQuery"/>.
/// </summary>
public class GetLibraryBookReadersQueryValidator : AbstractValidator<GetLibraryBookReadersQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryBookReadersQueryValidator"/> class.
    /// </summary>
    public GetLibraryBookReadersQueryValidator()
    {
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }
}
