#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for language information.
/// </summary>
/// <param name="LanguageCode">The ISO code of the language (e.g., "en" for English). Required.</param>
/// <param name="LanguageName">The name of the language in English. Required.</param>
/// <param name="NativeName">The native name of the language (e.g., "Español" for Spanish). Optional.</param>
[DebuggerDisplay("LanguageName: {LanguageName}")]
public record LanguageInfoDto(
    string? LanguageCode,
    string? LanguageName,
    string? NativeName
);
