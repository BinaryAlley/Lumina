namespace Lumina.Application.Common.Models.Common;

/// <summary>
/// Represents a request to get language information.
/// </summary>
public record LanguageInfoDto(
    string LanguageCode,
    string LanguageName,
    string? NativeName
);