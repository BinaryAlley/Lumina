#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="UpdateLibraryRequest"/>.
/// </summary>
public static class UpdateLibraryRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdateLibraryCommand"/>.
    /// </summary>
    /// <remarks>
    /// A path template part that is <see langword="null"/> is converted to a part without a kind, and a provider or book reader choice that is <see langword="null"/> is
    /// converted to a choice without a plugin Id, so that the validation of the command reports them, instead of the conversion failing before the command is validated.
    /// </remarks>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static UpdateLibraryCommand ToCommand(this UpdateLibraryRequest request)
    {
        return new UpdateLibraryCommand(
            request.Id,
            request.Title,
            request.LibraryType,
            request.ContentLocations,
            request.CoverImage,
            request.IsEnabled,
            request.IsLocked,
            request.CanDownloadMetadataFromWeb,
            request.ShouldSaveMetadataInMediaDirectories,
            request.ShouldSkipUnchangedDirectoriesDuringScan,
            request.IsContentOnSsd,
            request.ShouldTrackConsumptionProgress,
            request.CompletionThresholdPercentage,
            request.PathTemplateParts?.Select(part => new LibraryPathTemplatePartDto(part?.Kind, part?.Representation, part?.IsOptional ?? false)).ToArray(),
            request.MetadataProviders?.Select(provider => new LibraryProviderSelectionDto(provider?.PluginId ?? Guid.Empty, provider?.IsEnabled)).ToArray(),
            request.ArtworkProviders?.Select(provider => new LibraryProviderSelectionDto(provider?.PluginId ?? Guid.Empty, provider?.IsEnabled)).ToArray(),
            request.BookReaders?.Select(reader => new LibraryProviderSelectionDto(reader?.PluginId ?? Guid.Empty, reader?.IsEnabled)).ToArray()
        );
    }
}
