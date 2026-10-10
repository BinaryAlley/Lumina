#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Value Object for the values captured from the relative path of a media library item, keyed by the semantic kind of the path part that produced them.
/// </summary>
public class ParsedLibraryPath : ValueObject
{
    private readonly IReadOnlyDictionary<LibraryPathPartKind, string> _values;

    /// <summary>
    /// Gets the captured values, keyed by their part kind.
    /// </summary>
    public IReadOnlyDictionary<LibraryPathPartKind, string> Values => _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedLibraryPath"/> class.
    /// </summary>
    /// <param name="values">The captured values, keyed by their part kind.</param>
    private ParsedLibraryPath(IReadOnlyDictionary<LibraryPathPartKind, string> values)
    {
        _values = values;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ParsedLibraryPath"/> class.
    /// </summary>
    /// <param name="values">The captured values, keyed by their part kind.</param>
    /// <returns>The created parsed path.</returns>
    public static ParsedLibraryPath Create(IReadOnlyDictionary<LibraryPathPartKind, string> values)
    {
        return new ParsedLibraryPath(values);
    }

    /// <summary>
    /// Gets the captured value of the part of the provided <paramref name="kind"/>.
    /// </summary>
    /// <param name="kind">The kind of the part whose value is retrieved.</param>
    /// <returns>An <see cref="Optional{TValue}"/> containing the captured value, or no value when the part was not captured.</returns>
    public Optional<string> GetString(LibraryPathPartKind kind)
    {
        return _values.TryGetValue(kind, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? Optional<string>.Some(value.Trim())
            : Optional<string>.None();
    }

    /// <summary>
    /// Gets the captured integer value of the part of the provided <paramref name="kind"/>.
    /// </summary>
    /// <param name="kind">The kind of the part whose value is retrieved.</param>
    /// <returns>An <see cref="Optional{TValue}"/> containing the captured integer, or no value when the part was not captured or is not an integer.</returns>
    public Optional<int> GetInt(LibraryPathPartKind kind)
    {
        return _values.TryGetValue(kind, out string? value) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
            ? Optional<int>.Some(parsed)
            : Optional<int>.None();
    }

    /// <summary>
    /// Gets the captured values as a dictionary, for callers that need to iterate over all of them.
    /// </summary>
    /// <returns>The captured values keyed by part kind.</returns>
    public IReadOnlyDictionary<LibraryPathPartKind, string> ToDictionary()
    {
        return _values.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object?> GetEqualityComponents()
    {
        // the captured values are stored in a dictionary, so they are ordered by their kind to keep equality independent of insertion order.
        foreach (KeyValuePair<LibraryPathPartKind, string> capturedValue in _values.OrderBy(capturedValue => capturedValue.Key))
        {
            yield return capturedValue.Key;
            yield return capturedValue.Value;
        }
    }
}
