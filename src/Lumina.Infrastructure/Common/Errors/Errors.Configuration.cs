#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Infrastructure.Common.Errors;

/// <summary>
/// Application configuration error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Configuration error types.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Gets the error that reports a validation failure when the database connection string is empty.
        /// </summary>
        public static Error DatabaseConnectionStringCannotBeEmpty => Error.Validation(description: nameof(DatabaseConnectionStringCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the media root directory is empty.
        /// </summary>
        public static Error MediaRootDirectoryCannotBeEmpty => Error.Validation(description: nameof(MediaRootDirectoryCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the media libraries directory is empty.
        /// </summary>
        public static Error MediaLibrariesDirectoryCannotBeEmpty => Error.Validation(description: nameof(MediaLibrariesDirectoryCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the media books directory is empty.
        /// </summary>
        public static Error MediaBooksDirectoryCannotBeEmpty => Error.Validation(description: nameof(MediaBooksDirectoryCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the media music directory is empty.
        /// </summary>
        public static Error MediaMusicDirectoryCannotBeEmpty => Error.Validation(description: nameof(MediaMusicDirectoryCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the encryption secret key is empty.
        /// </summary>
        public static Error EncryptionSecretKeyCannotBeEmpty => Error.Validation(description: nameof(EncryptionSecretKeyCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the encryption secret key is not a valid Base64 string.
        /// </summary>
        public static Error EncryptionSecretKeyMustBeABase64String => Error.Validation(description: nameof(EncryptionSecretKeyMustBeABase64String));

        /// <summary>
        /// Gets the error that reports a validation failure when the JWT secret key is empty.
        /// </summary>
        public static Error JwtSecretKeyCannotBeEmpty => Error.Validation(description: nameof(JwtSecretKeyCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the JWT secret key is shorter than the required 32 characters.
        /// </summary>
        public static Error JwtSecretKeyTooShort => Error.Validation(description: nameof(JwtSecretKeyTooShort));

        /// <summary>
        /// Gets the error that reports a validation failure when the JWT expiry duration, in minutes, is not a positive number.
        /// </summary>
        public static Error JwtExpiryMinutesMustBePositive => Error.Validation(description: nameof(JwtExpiryMinutesMustBePositive));

        /// <summary>
        /// Gets the error that reports a validation failure when the JWT issuer is empty.
        /// </summary>
        public static Error JwtIssuerCannotBeEmpty => Error.Validation(description: nameof(JwtIssuerCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the JWT audience is empty.
        /// </summary>
        public static Error JwtAudienceCannotBeEmpty => Error.Validation(description: nameof(JwtAudienceCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when no cross-origin resource sharing origin is configured, or when one of the configured origins is empty.
        /// </summary>
        public static Error CorsOriginsCannotBeEmpty => Error.Validation(description: nameof(CorsOriginsCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when a configured cross-origin resource sharing origin is not the wildcard origin, is not an absolute HTTP or HTTPS URL, or ends with a slash.
        /// </summary>
        public static Error CorsOriginIsInvalid => Error.Validation(description: nameof(CorsOriginIsInvalid));

        /// <summary>
        /// Gets the error that reports a validation failure when the plugins directory is empty.
        /// </summary>
        public static Error PluginsDirectoryCannotBeEmpty => Error.Validation(description: nameof(PluginsDirectoryCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the theme storage path is empty.
        /// </summary>
        public static Error ThemeStoragePathCannotBeEmpty => Error.Validation(description: nameof(ThemeStoragePathCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the path of the bundled themes is empty.
        /// </summary>
        public static Error ThemeBundledThemesPathCannotBeEmpty => Error.Validation(description: nameof(ThemeBundledThemesPathCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the identifier of the default theme is empty.
        /// </summary>
        public static Error ThemeDefaultThemeIdCannotBeEmpty => Error.Validation(description: nameof(ThemeDefaultThemeIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the maximum allowed size of a theme archive, in bytes, is not a positive number.
        /// </summary>
        public static Error ThemeMaxArchiveBytesMustBePositive => Error.Validation(description: nameof(ThemeMaxArchiveBytesMustBePositive));

        /// <summary>
        /// Gets the error that reports a validation failure when the maximum allowed expanded size of a theme pack, in bytes, is not a positive number.
        /// </summary>
        public static Error ThemeMaxExpandedBytesMustBePositive => Error.Validation(description: nameof(ThemeMaxExpandedBytesMustBePositive));

        /// <summary>
        /// Gets the error that reports a validation failure when the maximum allowed size of a single file in a theme pack, in bytes, is not a positive number.
        /// </summary>
        public static Error ThemeMaxSingleFileBytesMustBePositive => Error.Validation(description: nameof(ThemeMaxSingleFileBytesMustBePositive));

        /// <summary>
        /// Gets the error that reports a validation failure when the maximum allowed number of entries in a theme pack archive is not a positive number.
        /// </summary>
        public static Error ThemeMaxEntriesMustBePositive => Error.Validation(description: nameof(ThemeMaxEntriesMustBePositive));

        /// <summary>
        /// Gets the error that reports a validation failure when the path of the ffmpeg binary is empty.
        /// </summary>
        public static Error TranscodingFfmpegPathCannotBeEmpty => Error.Validation(description: nameof(TranscodingFfmpegPathCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports a validation failure when the maximum number of concurrent normalization jobs is not a positive number.
        /// </summary>
        public static Error TranscodingMaxConcurrentJobsMustBePositive => Error.Validation(description: nameof(TranscodingMaxConcurrentJobsMustBePositive));

        /// <summary>
        /// Gets the error that reports a validation failure when the maximum number of concurrent normalization jobs exceeds 64.
        /// </summary>
        public static Error TranscodingMaxConcurrentJobsIsTooLarge => Error.Validation(description: nameof(TranscodingMaxConcurrentJobsIsTooLarge));

        /// <summary>
        /// Gets the error that reports a validation failure when the maximum number of concurrent normalization jobs per user is not a positive number.
        /// </summary>
        public static Error TranscodingMaxConcurrentJobsPerUserMustBePositive => Error.Validation(description: nameof(TranscodingMaxConcurrentJobsPerUserMustBePositive));

        /// <summary>
        /// Gets the error that reports a validation failure when the maximum number of concurrent normalization jobs per user exceeds 64.
        /// </summary>
        public static Error TranscodingMaxConcurrentJobsPerUserIsTooLarge => Error.Validation(description: nameof(TranscodingMaxConcurrentJobsPerUserIsTooLarge));

        /// <summary>
        /// Gets the error that reports a validation failure when the maximum number of concurrent normalization jobs per user exceeds the global maximum.
        /// </summary>
        public static Error TranscodingMaxConcurrentJobsPerUserExceedsGlobal => Error.Validation(description: nameof(TranscodingMaxConcurrentJobsPerUserExceedsGlobal));

        /// <summary>
        /// Gets the error that reports a validation failure when the maximum size of the normalized audio cache, in bytes, is not a positive number.
        /// </summary>
        public static Error TranscodingMaxCacheBytesMustBePositive => Error.Validation(description: nameof(TranscodingMaxCacheBytesMustBePositive));

        /// <summary>
        /// Gets the error that reports a validation failure when the normalization job timeout, in seconds, is not a positive number.
        /// </summary>
        public static Error TranscodingJobTimeoutMustBePositive => Error.Validation(description: nameof(TranscodingJobTimeoutMustBePositive));

        /// <summary>
        /// Gets the error that reports a validation failure when the normalization job timeout exceeds 86400 seconds.
        /// </summary>
        public static Error TranscodingJobTimeoutIsTooLong => Error.Validation(description: nameof(TranscodingJobTimeoutIsTooLong));

        /// <summary>
        /// Gets the error that reports a validation failure when the eviction grace period of the normalized audio cache, in seconds, is not a positive number.
        /// </summary>
        public static Error TranscodingEvictionGraceSecondsMustBePositive => Error.Validation(description: nameof(TranscodingEvictionGraceSecondsMustBePositive));

        /// <summary>
        /// Gets the error that reports a validation failure when the eviction grace period of the normalized audio cache exceeds 86400 seconds.
        /// </summary>
        public static Error TranscodingEvictionGraceSecondsIsTooLong => Error.Validation(description: nameof(TranscodingEvictionGraceSecondsIsTooLong));

        /// <summary>
        /// Gets the error that reports a validation failure when the configured normalized audio cache directory is not a valid file system path.
        /// </summary>
        public static Error TranscodingCacheDirectoryIsInvalid => Error.Validation(description: nameof(TranscodingCacheDirectoryIsInvalid));

        /// <summary>
        /// Gets the error that reports a validation failure when the configured normalized audio cache directory is inside the media root, or is the media root itself.
        /// </summary>
        public static Error TranscodingCacheDirectoryIsInsideMediaRoot => Error.Validation(description: nameof(TranscodingCacheDirectoryIsInsideMediaRoot));

        /// <summary>
        /// Gets the error that reports a validation failure when the configured normalized audio cache directory contains the media root.
        /// </summary>
        public static Error TranscodingCacheDirectoryContainsMediaRoot => Error.Validation(description: nameof(TranscodingCacheDirectoryContainsMediaRoot));

        /// <summary>
        /// Gets the error that reports a validation failure when the configured normalized audio cache directory is a file system root, the application base directory or one of its ancestors, or overlaps the logs or plugins directories.
        /// </summary>
        public static Error TranscodingCacheDirectoryIsUnsafeLocation => Error.Validation(description: nameof(TranscodingCacheDirectoryIsUnsafeLocation));

        /// <summary>
        /// Gets the error that reports a validation failure when the default normalized audio cache directory, used when no cache directory is configured, is inside the media root.
        /// </summary>
        public static Error TranscodingDefaultCacheDirectoryIsInsideMediaRoot => Error.Validation(description: nameof(TranscodingDefaultCacheDirectoryIsInsideMediaRoot));
    }
}
