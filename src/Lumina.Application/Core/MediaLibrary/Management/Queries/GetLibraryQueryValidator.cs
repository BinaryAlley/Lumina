#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries;

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
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(Errors.Users.UserIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Users.UserIdCannotBeEmpty.Description);
    }
}
