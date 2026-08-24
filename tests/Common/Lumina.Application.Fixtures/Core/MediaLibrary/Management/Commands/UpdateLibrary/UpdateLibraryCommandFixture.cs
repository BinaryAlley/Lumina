#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.UpdateLibrary;

/// <summary>
/// Fixture class for the <see cref="UpdateLibraryCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateLibraryCommandFixture
{
    /// <summary>
    /// Creates a random valid command to update a media library.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library.</param>
    /// <param name="ownerId">Optional. The Id of the user owning the media library.</param>
    /// <param name="title">Optional. The title of the media library.</param>
    /// <param name="libraryType">Optional. The type of the media library.</param>
    /// <param name="contentLocations">Optional. The content locations of the media library.</param>
    /// <param name="coverImage">Optional. The cover image path of the media library.</param>
    /// <param name="isEnabled">Whether the media library is enabled or not.</param>
    /// <param name="isLocked">Whether the media library is locked or not.</param>
    /// <param name="canDownloadMetadataFromWeb">Whether the media library should download metadata from the web or not.</param>
    /// <param name="shouldSaveMetadataInMediaDirectories">Whether the metadata should be saved in the media directories or not.</param>
    /// <param name="shouldSkipUnchangedDirectoriesDuringScan">Whether unchanged directories should be skipped during a scan or not.</param>
    /// <returns>The created command.</returns>
    public UpdateLibraryCommand Create(
        Guid? id = null,
        Guid? ownerId = null,
        string? title = null,
        string? libraryType = null,
        string[]? contentLocations = null,
        string? coverImage = null,
        bool isEnabled = true,
        bool isLocked = false,
        bool canDownloadMetadataFromWeb = true,
        bool shouldSaveMetadataInMediaDirectories = true,
        bool shouldSkipUnchangedDirectoriesDuringScan = false)
    {
        return new Faker<UpdateLibraryCommand>()
            .CustomInstantiator(f => new UpdateLibraryCommand(
                default,
                default,
                default!,
                default!,
                default!,
                default!,
                true,
                false,
                true,
                true,
                false
            ))
            .RuleFor(x => x.Id, id ?? Guid.NewGuid())
            .RuleFor(x => x.OwnerId, ownerId ?? Guid.NewGuid())
            .RuleFor(x => x.Title, f => title ?? f.Lorem.Word())
            .RuleFor(x => x.LibraryType, f => libraryType ?? f.Random.Enum<LibraryType>().ToString())
            .RuleFor(x => x.ContentLocations, f => contentLocations ?? [f.System.DirectoryPath(), f.System.DirectoryPath(), f.System.DirectoryPath()])
            .RuleFor(x => x.CoverImage, f => coverImage ?? f.System.FilePath())
            .RuleFor(x => x.IsEnabled, isEnabled)
            .RuleFor(x => x.IsLocked, isLocked)
            .RuleFor(x => x.CanDownloadMetadataFromWeb, canDownloadMetadataFromWeb)
            .RuleFor(x => x.ShouldSaveMetadataInMediaDirectories, shouldSaveMetadataInMediaDirectories)
            .RuleFor(x => x.ShouldSkipUnchangedDirectoriesDuringScan, shouldSkipUnchangedDirectoriesDuringScan)
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateLibraryCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateLibraryCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
