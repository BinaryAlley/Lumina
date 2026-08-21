#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Infrastructure.Common.Errors;

/// <summary>
/// Application configuration error types.
/// </summary>
public static partial class Errors
{
    public static class Configuration
    {
        public static Error DatabaseConnectionStringCannotBeEmpty => Error.Validation(description: nameof(DatabaseConnectionStringCannotBeEmpty));
        public static Error MediaRootDirectoryCannotBeEmpty => Error.Validation(description: nameof(MediaRootDirectoryCannotBeEmpty));
        public static Error MediaLibrariesDirectoryCannotBeEmpty => Error.Validation(description: nameof(MediaLibrariesDirectoryCannotBeEmpty));
        public static Error EncryptionSecretKeyCannotBeEmpty => Error.Validation(description: nameof(EncryptionSecretKeyCannotBeEmpty));
        public static Error EncryptionSecretKeyMustBeABase64String => Error.Validation(description: nameof(EncryptionSecretKeyMustBeABase64String));
        public static Error JwtSecretKeyCannotBeEmpty => Error.Validation(description: nameof(JwtSecretKeyCannotBeEmpty));
        public static Error JwtSecretKeyTooShort => Error.Validation(description: nameof(JwtSecretKeyTooShort));
        public static Error JwtExpiryMinutesMustBePositive => Error.Validation(description: nameof(JwtExpiryMinutesMustBePositive));
        public static Error JwtIssuerCannotBeEmpty => Error.Validation(description: nameof(JwtIssuerCannotBeEmpty));
        public static Error JwtAudienceCannotBeEmpty => Error.Validation(description: nameof(JwtAudienceCannotBeEmpty));
        public static Error CorsOriginsCannotBeEmpty => Error.Validation(description: nameof(CorsOriginsCannotBeEmpty));
        public static Error CorsOriginIsInvalid => Error.Validation(description: nameof(CorsOriginIsInvalid));
        public static Error PluginsDirectoryCannotBeEmpty => Error.Validation(description: nameof(PluginsDirectoryCannotBeEmpty));
        public static Error ThemeStoragePathCannotBeEmpty => Error.Validation(description: nameof(ThemeStoragePathCannotBeEmpty));
        public static Error ThemeBundledThemesPathCannotBeEmpty => Error.Validation(description: nameof(ThemeBundledThemesPathCannotBeEmpty));
        public static Error ThemeDefaultThemeIdCannotBeEmpty => Error.Validation(description: nameof(ThemeDefaultThemeIdCannotBeEmpty));
        public static Error ThemeMaxArchiveBytesMustBePositive => Error.Validation(description: nameof(ThemeMaxArchiveBytesMustBePositive));
        public static Error ThemeMaxExpandedBytesMustBePositive => Error.Validation(description: nameof(ThemeMaxExpandedBytesMustBePositive));
        public static Error ThemeMaxSingleFileBytesMustBePositive => Error.Validation(description: nameof(ThemeMaxSingleFileBytesMustBePositive));
        public static Error ThemeMaxEntriesMustBePositive => Error.Validation(description: nameof(ThemeMaxEntriesMustBePositive));
    }
}
