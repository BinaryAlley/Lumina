#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
#endregion

namespace Nemera.Application.Core.Plugins.Queries.GetLibraryTypeBookReaders;

/// <summary>
/// Validates the needed validation rules for <see cref="GetLibraryTypeBookReadersQuery"/>.
/// </summary>
public class GetLibraryTypeBookReadersQueryValidator : AbstractValidator<GetLibraryTypeBookReadersQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeBookReadersQueryValidator"/> class.
    /// </summary>
    public GetLibraryTypeBookReadersQueryValidator()
    {
        RuleFor(query => query.LibraryType)
            .NotNull()
            .WithError(Errors.Library.LibraryTypeCannotBeNull)
            .IsInEnum<GetLibraryTypeBookReadersQuery, LibraryType>()
            .WithError(Errors.Library.UnknownLibraryType);
    }
}
