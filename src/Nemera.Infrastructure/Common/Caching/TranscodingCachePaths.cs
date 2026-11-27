#region ========================================================================= USING =====================================================================================
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
#endregion

namespace Lumina.Infrastructure.Common.Caching;

/// <summary>
/// Provides the location, the ownership marker and the permissions of the temporary directory into which the audio normalization pipeline writes the normalized audio.
/// </summary>
internal static class TranscodingCachePaths
{
    /// <summary>
    /// The name of the temporary directory that is used when no cache directory is configured.
    /// </summary>
    public const string DEFAULT_DIRECTORY_NAME = "lumina-transcoding-cache";

    /// <summary>
    /// The name of the file that marks a directory as a normalized audio cache created and owned by this application.
    /// </summary>
    public const string CACHE_MARKER_FILE_NAME = ".lumina-transcoding-cache";

    /// <summary>
    /// The prefix of the temporary directories of the normalization jobs that have not been published to the cache yet.
    /// </summary>
    public const string TEMPORARY_DIRECTORY_PREFIX = "tmp-";

    /// <summary>
    /// The prefix of the tombstone directories that were detached from the cache and are waiting to be deleted.
    /// </summary>
    public const string DELETION_DIRECTORY_PREFIX = "del-";

    private const int CACHE_ID_HEX_LENGTH = 64;
    private const UnixFileMode PRIVATE_DIRECTORY_MODE = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private const UnixFileMode PRIVATE_FILE_MODE = UnixFileMode.UserRead | UnixFileMode.UserWrite;
    private const int USER_SUFFIX_LENGTH = 16;

    /// <summary>
    /// Gets the absolute path of the directory into which the audio normalization pipeline writes the normalized audio of the tracks.
    /// </summary>
    /// <param name="configuredDirectory">
    /// The configured cache directory, relative to the application base directory when not rooted, or <see langword="null"/> to use a private directory
    /// under the system temporary directory. The default directory is scoped to the operating system account that runs the process.
    /// </param>
    /// <returns>The absolute path of the transcoding cache directory.</returns>
    public static string GetRootDirectory(string? configuredDirectory)
    {
        if (!string.IsNullOrWhiteSpace(configuredDirectory))
        {
            string trimmedDirectory = configuredDirectory.Trim();
            string absoluteDirectory = Path.IsPathRooted(trimmedDirectory)
                ? trimmedDirectory
                : Path.Combine(AppContext.BaseDirectory, trimmedDirectory);
            return Path.GetFullPath(absoluteDirectory);
        }
        return Path.Combine(Path.GetTempPath(), DEFAULT_DIRECTORY_NAME + "-" + GetUserSuffix());
    }

    /// <summary>
    /// Checks whether a directory carries the marker of a normalized audio cache created by this application.
    /// </summary>
    /// <param name="directoryPath">The absolute path of the directory to check.</param>
    /// <returns><see langword="true"/> when the directory carries the cache marker, otherwise <see langword="false"/>.</returns>
    public static bool HasCacheMarker(string directoryPath)
    {
        return File.Exists(Path.Combine(directoryPath, CACHE_MARKER_FILE_NAME));
    }

    /// <summary>
    /// Writes the marker that records the directory as a normalized audio cache created and owned by this application.
    /// </summary>
    /// <param name="directoryPath">The absolute path of the directory to mark.</param>
    /// <remarks>
    /// The marker is an ownership signal, not a lock: it is written once, and its presence is what allows a later startup to clean the directory. A directory
    /// without the marker is never cleaned, so a cache directory that was configured by mistake can never lose data that does not belong to this application.
    /// </remarks>
    public static void WriteCacheMarker(string directoryPath)
    {
        if (HasCacheMarker(directoryPath))
            return;
        File.WriteAllText(Path.Combine(directoryPath, CACHE_MARKER_FILE_NAME), "Lumina normalized audio cache." + Environment.NewLine);
    }

    /// <summary>
    /// Checks whether an entry name belongs to the namespace of the normalized audio cache.
    /// </summary>
    /// <param name="entryName">The name of the entry, without its directory.</param>
    /// <returns><see langword="true"/> when the name is a cache identifier, a temporary directory name or a deletion tombstone name, otherwise <see langword="false"/>.</returns>
    public static bool IsCacheEntryName(string entryName)
    {
        if (string.IsNullOrEmpty(entryName))
            return false;
        return IsValidCacheId(entryName) ||
               entryName.StartsWith(TEMPORARY_DIRECTORY_PREFIX, StringComparison.Ordinal) ||
               entryName.StartsWith(DELETION_DIRECTORY_PREFIX, StringComparison.Ordinal);
    }

    /// <summary>
    /// Checks whether a value is a well formed opaque cache identifier.
    /// </summary>
    /// <param name="cacheId">The value to check.</param>
    /// <returns><see langword="true"/> when the value is a well formed cache identifier, otherwise <see langword="false"/>.</returns>
    public static bool IsValidCacheId(string cacheId)
    {
        if (string.IsNullOrEmpty(cacheId) || cacheId.Length != CACHE_ID_HEX_LENGTH)
            return false;
        foreach (char character in cacheId)
            if (!char.IsAsciiHexDigitLower(character))
                return false;
        return true;
    }

    /// <summary>
    /// Removes the entries of a directory that belong to the namespace of the normalized audio cache, and leaves every other entry untouched.
    /// </summary>
    /// <param name="directoryPath">The absolute path of the cache directory.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <remarks>
    /// The cache directory itself is never deleted, and entries that do not carry a cache identifier, a temporary directory name or a deletion tombstone
    /// name are never inspected. This makes the startup cleanup safe even when the configured directory is not one this application created.
    /// </remarks>
    /// <exception cref="OperationCanceledException">The cleanup was canceled.</exception>
    /// <exception cref="IOException">An entry could not be deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">An entry could not be deleted because access was denied.</exception>
    public static void ClearCacheEntries(string directoryPath, CancellationToken cancellationToken)
    {
        foreach (string entryPath in Directory.GetDirectories(directoryPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsCacheEntryName(Path.GetFileName(entryPath)))
                continue;
            DirectoryInfo entryDirectory = new(entryPath);
            // A cache entry is never a link. A link left under a cache like name is deleted as a link, so the deletion can never be redirected outside the cache.
            if (entryDirectory.LinkTarget is not null)
                entryDirectory.Delete();
            else
                DeleteDirectoryRecursive(entryPath, cancellationToken);
        }
    }

    /// <summary>
    /// Deletes a directory and everything it contains.
    /// </summary>
    /// <param name="directoryPath">The directory to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <exception cref="OperationCanceledException">The deletion was canceled.</exception>
    /// <exception cref="IOException">A file or directory could not be deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">A file or directory could not be deleted because access was denied.</exception>
    private static void DeleteDirectoryRecursive(string directoryPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (string filePath in Directory.EnumerateFiles(directoryPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Delete(filePath);
        }
        foreach (string childDirectoryPath in Directory.EnumerateDirectories(directoryPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            DirectoryInfo childDirectory = new(childDirectoryPath);
            // A link is deleted as a link, so a tampered entry can never make the cleanup follow it outside the cache.
            if (childDirectory.LinkTarget is not null)
                childDirectory.Delete();
            else
                DeleteDirectoryRecursive(childDirectoryPath, cancellationToken);
        }
        cancellationToken.ThrowIfCancellationRequested();
        Directory.Delete(directoryPath);
    }

    /// <summary>
    /// Verifies that an existing cache directory is not a symbolic link and tightens its permissions to the private mode on Unix.
    /// </summary>
    /// <param name="directoryPath">The absolute path of the cache directory.</param>
    /// <exception cref="IOException">The path is a symbolic link or cannot be inspected.</exception>
    /// <exception cref="UnauthorizedAccessException">The directory is owned by another user or cannot be changed.</exception>
    public static void RestrictPermissions(string directoryPath)
    {
        DirectoryInfo directoryInformation = new(directoryPath);
        // A symbolic link is rejected even when its target does not exist, so CreateDirectory can never be redirected outside the intended path.
        if (directoryInformation.LinkTarget is not null)
            throw new IOException("The normalized audio cache directory is a symbolic link.");

        // On Unix a directory that already exists is not changed by CreateDirectory, and a directory planted by another user under a predictable name in
        // the shared temporary directory would otherwise be reused with its own mode. Tightening it fails when the directory is not owned by this
        // account, which makes the caller treat the cache as unavailable instead of using a directory it does not control.
        if (!OperatingSystem.IsWindows() && directoryInformation.Exists)
            File.SetUnixFileMode(directoryPath, PRIVATE_DIRECTORY_MODE);
    }

    /// <summary>
    /// Tightens the permissions of a file the cache created to owner read and write on Unix.
    /// </summary>
    /// <param name="filePath">The absolute path of the file.</param>
    /// <exception cref="IOException">The file permissions could not be changed.</exception>
    /// <exception cref="UnauthorizedAccessException">The file is owned by another user or cannot be changed.</exception>
    public static void RestrictFilePermissions(string filePath)
    {
        // Windows inherits the ACL of the private cache directory, so only the Unix mode bits need to be tightened here.
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(filePath, PRIVATE_FILE_MODE);
    }

    /// <summary>
    /// Creates a directory with private permissions on the platforms that support them.
    /// </summary>
    /// <param name="directoryPath">The absolute path of the directory to create.</param>
    /// <exception cref="IOException">The directory could not be created.</exception>
    /// <exception cref="UnauthorizedAccessException">The directory could not be created because access was denied.</exception>
    public static void CreatePrivateDirectory(string directoryPath)
    {
        if (OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(directoryPath);
            return;
        }
        Directory.CreateDirectory(directoryPath, PRIVATE_DIRECTORY_MODE);
        // CreateDirectory does not change an existing directory, so the mode is applied again after the directory is known to exist.
        File.SetUnixFileMode(directoryPath, PRIVATE_DIRECTORY_MODE);
    }

    /// <summary>
    /// Creates the cache directory when it does not exist, or tightens an existing one, with private permissions on the platforms that support them.
    /// </summary>
    /// <param name="directoryPath">The absolute path of the cache directory.</param>
    /// <remarks>
    /// A directory created by this method is marked as owned by this application. An existing directory is never marked, so an operator supplied directory
    /// is adopted only by the startup component, and only when it is empty.
    /// </remarks>
    /// <exception cref="IOException">The directory could not be created, or the path is a symbolic link.</exception>
    /// <exception cref="UnauthorizedAccessException">The directory could not be created or changed because access was denied.</exception>
    public static void EnsureDirectoryExists(string directoryPath)
    {
        RestrictPermissions(directoryPath);
        bool isCreated = !Directory.Exists(directoryPath);
        CreatePrivateDirectory(directoryPath);
        if (isCreated)
            WriteCacheMarker(directoryPath);
    }

    /// <summary>
    /// Deletes a directory, and reports whether it was deleted.
    /// </summary>
    /// <param name="directoryPath">The directory to delete.</param>
    /// <returns><see langword="true"/> when the directory no longer exists, otherwise <see langword="false"/>.</returns>
    public static bool TryDeleteDirectory(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, recursive: true);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a short, file name safe suffix of the operating system account that runs the process, so the default directory is not shared between users.
    /// </summary>
    /// <returns>The account suffix used in the default cache directory name.</returns>
    private static string GetUserSuffix()
    {
        string userIdentity = Environment.UserName;
        if (string.IsNullOrWhiteSpace(userIdentity))
            userIdentity = "default";
        byte[] identityHash = SHA256.HashData(Encoding.UTF8.GetBytes(userIdentity));
        return Convert.ToHexString(identityHash).ToLowerInvariant()[..USER_SUFFIX_LENGTH];
    }
}
