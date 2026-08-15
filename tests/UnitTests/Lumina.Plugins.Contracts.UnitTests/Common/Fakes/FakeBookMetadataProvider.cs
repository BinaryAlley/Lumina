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

namespace Lumina.Plugins.Contracts.UnitTests.Common.Fakes;

/// <summary>
/// A fake book metadata provider used to exercise the plugin contracts from outside.
/// </summary>
public sealed class FakeBookMetadataProvider : IPlugin, IPluginServiceRegistrator, IRemoteMetadataProvider<BookMetadataLookupDto, BookMetadataDto>
{
    /// <summary>
    /// Gets the stable unique identifier of the plugin, used to persist its state.
    /// </summary>
    public Guid Id { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name { get; } = "Fake Book Metadata Provider";

    /// <summary>
    /// Gets the author of the plugin.
    /// </summary>
    public string Author { get; } = "Lumina";

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    public Version Version { get; } = new(1, 0, 0);

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    public string Description { get; } = "A fake metadata provider used to exercise the plugin contracts.";

    /// <summary>
    /// Gets the media library type this metadata provider supports.
    /// </summary>
    public LibraryType SupportedLibraryType { get; } = LibraryType.Book;

    /// <summary>
    /// Gets a value indicating whether this metadata provider requires access to the web to retrieve metadata.
    /// </summary>
    public bool RequiresWebAccess { get; } = true;

    /// <summary>
    /// Gets the settings schema of the plugin, used by the host to render the plugin settings page.
    /// </summary>
    /// <returns>The collection of setting descriptors declared by the plugin.</returns>
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

    /// <summary>
    /// Registers the services of the plugin into the host dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to which the plugin services are added.</param>
    public void RegisterServices(IServiceCollection services)
    {
    }

    /// <summary>
    /// Searches for the metadata of the media item described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of metadata candidates found for the media item.</returns>
    public Task<IReadOnlyList<BookMetadataDto>> GetSearchResultsAsync(BookMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<BookMetadataDto>>(
            [
                CreateMetadata("The Fellowship of the Ring", "3")
            ]
        );
    }

    /// <summary>
    /// Gets the metadata of the media item described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the media item, or <see langword="null"/> when no metadata was found.</returns>
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
