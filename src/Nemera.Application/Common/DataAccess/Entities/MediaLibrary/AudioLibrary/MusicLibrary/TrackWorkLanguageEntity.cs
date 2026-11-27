#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for a language of the work a track is a recording of.
/// </summary>
/// <param name="LanguageCode">The ISO 639-1 two-letter language code.</param>
/// <param name="LanguageName">The full name of the language in English.</param>
/// <param name="NativeName">The native name of the language, if applicable.</param>
[DebuggerDisplay("LanguageCode: {LanguageCode}")]
public record TrackWorkLanguageEntity(
    string LanguageCode,
    string LanguageName,
    string? NativeName
);
