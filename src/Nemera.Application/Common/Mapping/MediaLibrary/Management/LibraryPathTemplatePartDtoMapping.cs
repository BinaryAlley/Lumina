#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="LibraryPathTemplatePartDto"/>.
/// </summary>
public static class LibraryPathTemplatePartDtoMapping
{
    /// <summary>
    /// Converts the provided path template parts to their domain path parts.
    /// </summary>
    /// <param name="parts">The path template parts to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the converted domain path parts, or an error message.
    /// </returns>
    public static Result<IReadOnlyList<LibraryPathPart>> ToDomainParts(this IEnumerable<LibraryPathTemplatePartDto>? parts)
    {
        List<LibraryPathPart> domainParts = [];
        if (parts is null)
            return domainParts;

        foreach (LibraryPathTemplatePartDto part in parts)
        {
            if (!Enum.TryParse(part.Kind, ignoreCase: true, out LibraryPathPartKind kind))
                return DomainErrors.Library.PathTemplatePartKindNotSupportedForLibraryType;

            Result<LibraryPathPart> partResult = LibraryPathPart.Create(kind, part.Representation, part.IsOptional);
            if (partResult.IsFailure)
                return partResult.Errors;

            domainParts.Add(partResult.Value);
        }
        return domainParts;
    }
}
