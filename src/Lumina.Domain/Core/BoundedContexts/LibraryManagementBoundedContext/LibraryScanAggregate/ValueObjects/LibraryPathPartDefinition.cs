#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Value Object that describes one available part of a media library path template, for a given library type.
/// </summary>
public sealed class LibraryPathPartDefinition : ValueObject
{
    /// <summary>
    /// Gets the kind of the described part.
    /// </summary>
    public LibraryPathPartKind Kind { get; }

    /// <summary>
    /// Gets the value type captured by the described part.
    /// </summary>
    public LibraryPathValueType ValueType { get; }

    /// <summary>
    /// Gets the default representation of the described part, pre-filled for the user when the part is added to a template.
    /// </summary>
    public string DefaultRepresentation { get; }

    /// <summary>
    /// Gets whether the described part is optional by default.
    /// </summary>
    public bool IsOptionalByDefault { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPathPartDefinition"/> class.
    /// </summary>
    /// <param name="kind">The kind of the described part.</param>
    /// <param name="valueType">The value type captured by the described part.</param>
    /// <param name="defaultRepresentation">The default representation of the described part.</param>
    /// <param name="isOptionalByDefault">Whether the described part is optional by default.</param>
    public LibraryPathPartDefinition(LibraryPathPartKind kind, LibraryPathValueType valueType, string defaultRepresentation, bool isOptionalByDefault)
    {
        Kind = kind;
        ValueType = valueType;
        DefaultRepresentation = defaultRepresentation;
        IsOptionalByDefault = isOptionalByDefault;
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return ValueType;
        yield return DefaultRepresentation;
        yield return IsOptionalByDefault;
    }
}
