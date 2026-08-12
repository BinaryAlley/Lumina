#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.UnitTests.Fakes;

/// <summary>
/// A fake book metadata provider used to exercise the plugin contracts from outside.
/// </summary>
public sealed class FakeBookMetadataProvider : IPlugin, IPluginServiceRegistrator, IRemoteMetadataProvider<BookMetadataLookupDto, BookMetadataDto>
{
    /// <inheritdoc/>
    public Guid Id { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <inheritdoc/>
    public string Name { get; } = "Fake Book Metadata Provider";

    /// <inheritdoc/>
    public string Author { get; } = "Lumina";

    /// <inheritdoc/>
    public Version Version { get; } = new(1, 0, 0);

    /// <inheritdoc/>
    public string Description { get; } = "A fake metadata provider used to exercise the plugin contracts.";

    /// <inheritdoc/>
    public LibraryType SupportedLibraryType { get; } = LibraryType.Book;

    /// <inheritdoc/>
    public bool RequiresWebAccess { get; } = true;

    /// <inheritdoc/>
    public IReadOnlyList<PluginSettingDescriptor> GetSettingsSchema()
    {
        return
        [
            new PluginSettingDescriptor(
                Key: "preferredLanguage",
                Label: "Preferred Language",
                Type: PluginSettingType.Text,
                DefaultValue: "en"
            ),
            new PluginSettingDescriptor(
                Key: "selectionStrategy",
                Label: "Selection Strategy",
                Type: PluginSettingType.Select,
                DefaultValue: "first",
                AllowedValues: ["first", "best"]
            )
        ];
    }

    /// <inheritdoc/>
    public void RegisterServices(IServiceCollection services)
    {
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<BookMetadataDto>> GetSearchResultsAsync(BookMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<BookMetadataDto>>(
            [
                CreateMetadata("The Fellowship of the Ring", "3")
            ]
        );
    }

    /// <inheritdoc/>
    public Task<BookMetadataDto?> GetMetadataAsync(BookMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        if (lookup.Title is null)
            return Task.FromResult<BookMetadataDto?>(null);
        return Task.FromResult<BookMetadataDto?>(CreateMetadata(lookup.Title, "3"));
    }

    private static BookMetadataDto CreateMetadata(string title, string goodreadsId)
    {
        return new BookMetadataDto(
            Title: title,
            OriginalTitle: title,
            Description: "A test description.",
            ReleaseInfo: new ReleaseInfoDto(
                OriginalReleaseDate: new DateOnly(1954, 7, 29),
                OriginalReleaseYear: 1954,
                ReReleaseDate: null,
                ReReleaseYear: null,
                ReleaseCountry: "uk",
                ReleaseVersion: null
            ),
            Genres: [new GenreDto("fantasy")],
            Tags: [new TagDto("epic fantasy")],
            Language: new LanguageInfoDto("en", "English", "English"),
            OriginalLanguage: null,
            Publisher: "Houghton Mifflin",
            PageCount: 398,
            Format: BookFormat.Paperback,
            Edition: null,
            VolumeNumber: 1,
            Series: null,
            ASIN: null,
            GoodreadsId: goodreadsId,
            LCCN: null,
            OCLCNumber: null,
            OpenLibraryId: null,
            LibraryThingId: null,
            GoogleBooksId: null,
            BarnesAndNobleId: null,
            AppleBooksId: null,
            Isbns: null,
            Contributors: null,
            Ratings: null,
            CoverImageUrl: null
        );
    }
}
