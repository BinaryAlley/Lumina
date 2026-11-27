#region ========================================================================= USING =====================================================================================
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.Fixtures.Core.Themes;

/// <summary>
/// Describes a single entry of a theme pack ZIP archive built by the <see cref="ThemePackFixture"/>.
/// </summary>
/// <param name="Path">The entry path inside the archive.</param>
/// <param name="Content">The text content of the entry.</param>
/// <param name="UnixFileType">Optional Unix file type for the entry, used to simulate special file types such as symbolic links.</param>
[ExcludeFromCodeCoverage]
public sealed record ThemeArchiveEntry(string Path, string Content, int? UnixFileType = null);
