#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Infrastructure.Common.Errors;

/// <summary>
/// Application configuration error types.
/// </summary>
public static partial class Errors
{
    public static class Configuration
    {
        public static Error ApplicationThemeCannotBeEmpty => Error.Validation(description: nameof(ApplicationThemeCannotBeEmpty));
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
    }
}
