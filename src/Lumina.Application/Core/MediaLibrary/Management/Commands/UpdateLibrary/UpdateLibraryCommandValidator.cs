#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
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
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.OwnerId)
            .NotEmpty()
            .WithError(Errors.Users.UserIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Users.UserIdCannotBeEmpty);

        RuleFor(command => command.LibraryType)
            .NotNull()
            .WithError(Errors.Library.LibraryTypeCannotBeNull)
            .Must(libraryType => Enum.TryParse<LibraryType>(libraryType, out _))
            .WithError(Errors.Library.UnknownLibraryType);

        RuleFor(command => command.ContentLocations)
            .NotNull()
            .WithError(Errors.Library.PathsListCannotBeNull)
            .NotEmpty()
            .WithError(Errors.Library.PathsListCannotBeEmpty);

        RuleForEach(command => command.ContentLocations)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty)
            .MaximumLength(260)
            .WithError(Errors.FileSystemManagement.PathMustBeMaximum260CharactersLong);

        RuleFor(command => command.Title)
            .NotEmpty()
            .WithError(Errors.Library.TitleCannotBeEmpty)
            .MaximumLength(255)
            .WithError(Errors.Library.TitleMustBeMaximum255CharactersLong);
    }
}
