#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.Filtering;

/// <summary>
/// Data transfer object that represents the parameters required to filter by library data.
/// </summary>
public record LibraryFilterDto : BaseFilterDto
{
    /// <summary>
    /// The Id of the media library used to filter data.
    /// </summary>
    public required Guid LibraryId { get; init; }

    /// <summary>
    /// The alpha key used to filter the data by the first character of the item title, for the alpha picker.
    /// Can be <see langword="null"/> to disable alpha filtering, a single ASCII letter (case-insensitive) to filter by that letter,
    /// <see cref="LibraryItemAlphaKeys.NUMBER"/> to filter by titles whose first character is a digit,
    /// or <see cref="LibraryItemAlphaKeys.SYMBOL"/> to filter by titles whose first character is neither a letter nor a digit.
    /// </summary>
    public string? FilterAlphaKey { get; init; }

    /// <summary>
    /// Whether the leading "The " prefix of a title should be ignored when computing the alpha key, or not.
    /// </summary>
    public bool ShouldIgnoreThePrefixForAlphaPicker { get; init; }
}
