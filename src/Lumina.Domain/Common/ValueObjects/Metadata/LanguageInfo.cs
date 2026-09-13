#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.Models.Core;

using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Value Object for the language used in a media element.
/// </summary>
[DebuggerDisplay("{LanguageCode}")]
public class LanguageInfo : ValueObject
{
    /// <summary>
    /// Gets the ISO 639-1 two-letter language code.
    /// </summary>
    public string LanguageCode { get; private set; }

    /// <summary>
    /// Gets the full name of the language in English.
    /// </summary>
    public string LanguageName { get; private set; }

    /// <summary>
    /// Gets an optional native name of the language.
    /// </summary>
    public Optional<string> NativeName { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageInfo"/> class.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 two-letter language code.</param>
    /// <param name="languageName">The full name of the language in English.</param>
    /// <param name="nativeName">The optional native name of the language.</param>
    private LanguageInfo(string languageCode, string languageName, Optional<string> nativeName)
    {
        LanguageCode = languageCode.ToLowerInvariant();
        LanguageName = languageName;
        NativeName = nativeName;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="LanguageInfo"/> class.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 two-letter language code.</param>
    /// <param name="languageName">The full name of the language in English.</param>
    /// <param name="nativeName">The optional native name of the language.</param>
    /// <returns>The created <see cref="LanguageInfo"/>.</returns>
    public static LanguageInfo Create(string languageCode, string languageName, Optional<string> nativeName)
    {
        return new LanguageInfo(languageCode, languageName, nativeName);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return LanguageCode;
        yield return LanguageName;
        yield return NativeName;
    }

    /// <summary>
    /// Customized ToString() method.
    /// </summary>
    /// <returns>Custom string value showing relevant data for current class.</returns>
    public override string ToString()
    {
        return $"{LanguageCode} - {LanguageName}";
    }
}
