#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Themes.Management.Commands.InstallTheme;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
#endregion

namespace Lumina.Application.Fixtures.Core.Themes.Management.Commands.InstallTheme;

/// <summary>
/// Fixture class for the <see cref="InstallThemeCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallThemeCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="InstallThemeCommand"/>.
    /// </summary>
    /// <param name="archive">Optional ZIP archive stream of the theme pack.</param>
    /// <param name="fileName">Optional file name of the uploaded archive.</param>
    /// <returns>The created <see cref="InstallThemeCommand"/>.</returns>
    public InstallThemeCommand Create(Stream? archive = null, string? fileName = null)
    {
        Stream resolvedArchive = archive ?? new MemoryStream(Encoding.UTF8.GetBytes(_faker.Lorem.Paragraph()));
        return new InstallThemeCommand(resolvedArchive, fileName ?? $"{_faker.Lorem.Word()}.zip");
    }

    /// <summary>
    /// Creates a list of <see cref="InstallThemeCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<InstallThemeCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
