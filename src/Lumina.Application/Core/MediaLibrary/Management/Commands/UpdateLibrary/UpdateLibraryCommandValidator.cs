#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdateLibraryCommand"/>.
/// </summary>
public class UpdateLibraryCommandValidator : AbstractValidator<UpdateLibraryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLibraryCommandValidator"/> class.
    /// </summary>
    public UpdateLibraryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description);
        
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage(Errors.Users.UserIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Users.UserIdCannotBeEmpty.Description);

        RuleFor(x => x.LibraryType)
            .NotNull().WithMessage(Errors.Library.LibraryTypeCannotBeNull.Description)
            .Must(x => Enum.TryParse<LibraryType>(x, out _))
            .WithMessage(Errors.Library.UnknownLibraryType.Description);

        RuleFor(x => x.ContentLocations)
            .NotNull().WithMessage(Errors.Library.PathsListCannotBeNull.Description)
            .NotEmpty().WithMessage(Errors.Library.PathsListCannotBeEmpty.Description);

        RuleForEach(x => x.ContentLocations)
            .NotEmpty().WithMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description)
            .MaximumLength(260).WithMessage(Errors.FileSystemManagement.PathMustBeMaximum260CharactersLong.Description);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(Errors.Library.TitleCannotBeEmpty.Description)
            .MaximumLength(255).WithMessage(Errors.Library.TitleMustBeMaximum255CharactersLong.Description);
    }
}
