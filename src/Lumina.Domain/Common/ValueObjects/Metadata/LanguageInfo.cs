#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
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
    #region ==================================================================== PROPERTIES =================================================================================
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
    #endregion

    #region ====================================================================== CTOR =====================================================================================
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
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="LanguageInfo"/> class.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 two-letter language code.</param>
    /// <param name="languageName">The full name of the language in English.</param>
    /// <param name="nativeName">The optional native name of the language.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="LanguageInfo"/>, or an error message.
    /// </returns>
    public static ErrorOr<LanguageInfo> Create(string languageCode, string languageName, Optional<string> nativeName)
    {
        return string.IsNullOrWhiteSpace(languageCode)
            ? (ErrorOr<LanguageInfo>)Errors.Errors.Metadata.LanguageCodeCannotBeEmpty
            : string.IsNullOrWhiteSpace(languageName)
            ? (ErrorOr<LanguageInfo>)Errors.Errors.Metadata.LanguageNameCannotBeEmpty
            : languageCode.Length != 2 ? (ErrorOr<LanguageInfo>)Errors.Errors.Metadata.InvalidIsoCode : (ErrorOr<LanguageInfo>)new LanguageInfo(languageCode, languageName, nativeName);
    }

    /// <inheritdoc/>
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
    #endregion
}