#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for language information.
/// </summary>
/// <param name="LanguageCode">Gets the ISO code of the language (e.g., "en" for English).</param>
/// <param name="LanguageName">Gets the name of the language in English.</param>
/// <param name="NativeName">Gets the native name of the language (e.g., "Español" for Spanish).</param>
[DebuggerDisplay("LanguageName: {LanguageName}")]
public record LanguageInfoDto(
    string? LanguageCode,
    string? LanguageName,
    string? NativeName
);
