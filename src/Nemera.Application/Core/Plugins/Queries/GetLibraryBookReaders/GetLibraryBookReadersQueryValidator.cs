#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryBookReaders;

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
