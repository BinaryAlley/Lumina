#region ========================================================================= USING =====================================================================================
using System;
using System.IO;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Provides the location of the temporary directory into which the reading plugins extract the contents of the books.
/// </summary>
public static class ReadingCachePaths
{
    /// <summary>
    /// The name of the temporary directory into which the reading plugins extract the contents of the books.
    /// </summary>
    public const string DIRECTORY_NAME = "reading-cache";

    /// <summary>
    /// Gets the absolute path of the temporary directory into which the reading plugins extract the contents of the books.
    /// </summary>
    /// <returns>The absolute path of the reading cache directory.</returns>
    public static string GetRootDirectory()
    {
        return Path.Combine(AppContext.BaseDirectory, DIRECTORY_NAME);
    }
}
