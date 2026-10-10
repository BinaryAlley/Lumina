#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for one part of a media library path template.
/// </summary>
/// <remarks>
/// A part is either a <see cref="LibraryPathPartKind.Separator"/>, which renders the platform path separator, a <see cref="LibraryPathPartKind.Literal"/>, which renders its 
/// <see cref="Representation"/> verbatim, or a typed value part, whose <see cref="Representation"/> is a mask that contains a single <c>{0}</c> placeholder where the captured value goes,
/// optionally with a fixed digit width for numeric kinds, like <c>{0:00}</c>.
/// </remarks>
public class LibraryPathPart : ValueObject
{
    /// <summary>
    /// Matches the value placeholder of a typed part, capturing the optional fixed digit width.
    /// </summary>
    private static readonly Regex s_placeholderRegex = new(@"\{0(?::(?<width>0+))?\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Gets the kind of the path part.
    /// </summary>
    public LibraryPathPartKind Kind { get; }

    /// <summary>
    /// Gets the representation of the path part, which is the literal text for a literal part, and the value mask for a typed part.
    /// </summary>
    public string Representation { get; }

    /// <summary>
    /// Gets whether the path part can be absent from the path.
    /// </summary>
    public bool IsOptional { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPathPart"/> class.
    /// </summary>
    /// <param name="kind">The kind of the path part.</param>
    /// <param name="representation">The literal text or the value mask of the path part.</param>
    /// <param name="isOptional">Whether the path part can be absent from the path.</param>
    private LibraryPathPart(LibraryPathPartKind kind, string representation, bool isOptional)
    {
        Kind = kind;
        Representation = representation;
        IsOptional = isOptional;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="LibraryPathPart"/> class.
    /// </summary>
    /// <param name="kind">The kind of the path part.</param>
    /// <param name="representation">The literal text or the value mask of the path part.</param>
    /// <param name="isOptional">Whether the path part can be absent from the path.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="LibraryPathPart"/>, or an error message.
    /// </returns>
    public static Result<LibraryPathPart> Create(LibraryPathPartKind kind, string? representation, bool isOptional)
    {
        // a separator carries no representation, it always renders the platform path separator.
        if (kind == LibraryPathPartKind.Separator)
            return new LibraryPathPart(kind, string.Empty, isOptional);

        if (string.IsNullOrWhiteSpace(representation))
            return DomainErrors.Library.PathTemplateLiteralCannotBeEmpty;

        // typed parts must carry exactly one value placeholder, so that the position of the captured value is unambiguous.
        if (kind != LibraryPathPartKind.Literal && s_placeholderRegex.Matches(representation).Count != 1)
            return DomainErrors.Library.PathTemplateValuePartMustContainSinglePlaceholder;

        return new LibraryPathPart(kind, representation, isOptional);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return Representation;
        yield return IsOptional;
    }
}
