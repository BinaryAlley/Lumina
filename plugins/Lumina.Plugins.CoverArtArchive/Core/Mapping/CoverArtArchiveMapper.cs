#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Core.Mapping;

/// <summary>
/// Maps the responses of the Cover Art Archive API into artwork DTOs.
/// </summary>
internal static class CoverArtArchiveMapper
{
    /// <summary>
    /// Maps the artwork carried by <paramref name="response"/> into artwork DTOs, one for every type each image is tagged with.
    /// </summary>
    /// <param name="response">The artwork response to map.</param>
    /// <returns>The mapped artworks, ordered as they appear in the response, or an empty collection when there is no artwork.</returns>
    public static IReadOnlyList<ArtworkDto> MapArtwork(CoverArtArchiveArtworkResponse? response)
    {
        // The collection and its elements are guarded, because the deserializer can produce a null collection, or null elements, regardless of the initializers.
        if (response?.Images is null)
            return [];

        List<ArtworkDto> artworks = [];
        // The ordinals are tracked per type, so that multiple images of the same type, like the pages of a booklet, are ordered among themselves.
        Dictionary<ArtworkType, int> nextOrdinals = [];
        foreach (CoverArtArchiveImageResponse? image in response.Images)
        {
            if (image is null || !TryResolveArtworkUrl(image.Image, out string artworkUrl))
                continue;

            foreach (ArtworkType artworkType in ResolveArtworkTypes(image))
            {
                int ordinal = nextOrdinals.TryGetValue(artworkType, out int nextOrdinal) ? nextOrdinal : 0;
                nextOrdinals[artworkType] = ordinal + 1;
                artworks.Add(new ArtworkDto(artworkType, ordinal, LocalPath: null, RemoteUrl: artworkUrl));
            }
        }
        return artworks;
    }

    /// <summary>
    /// Resolves the URL of an artwork image, keeping it only when it points, over an encrypted connection, at a host the Cover Art Archive may serve
    /// artwork from. An image URL of any other shape or host is dropped, so a compromised response cannot make the host download from an internal address.
    /// </summary>
    /// <param name="candidate">The image URL carried by the response.</param>
    /// <param name="artworkUrl">The resolved, encrypted image URL, when it is usable.</param>
    /// <returns><see langword="true"/> when the image URL is usable, otherwise <see langword="false"/>.</returns>
    private static bool TryResolveArtworkUrl(string? candidate, out string artworkUrl)
    {
        artworkUrl = string.Empty;
        if (string.IsNullOrWhiteSpace(candidate))
            return false;

        if (!Uri.TryCreate(candidate.Trim(), UriKind.Absolute, out Uri? uri) || !CoverArtArchiveHosts.IsAllowed(uri.Host))
            return false;

        // The artwork is always fetched over an encrypted connection, so a plain HTTP reference to a trusted host is upgraded to HTTPS,
        // and a reference that uses any other scheme is dropped.
        if (uri.Scheme == Uri.UriSchemeHttps)
            artworkUrl = uri.AbsoluteUri;
        else if (uri.Scheme == Uri.UriSchemeHttp)
            artworkUrl = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri.AbsoluteUri;
        else
            return false;

        return true;
    }

    /// <summary>
    /// Resolves the artwork types an image is tagged with, falling back to the front and back flags when the image carries no type.
    /// </summary>
    /// <param name="image">The image whose types are resolved.</param>
    /// <returns>The distinct artwork types of the image, in the order they appear in the response.</returns>
    private static List<ArtworkType> ResolveArtworkTypes(CoverArtArchiveImageResponse image)
    {
        // A list de-duplicated on insertion keeps the order of the response, unlike a set, whose iteration order is not deterministic.
        List<ArtworkType> artworkTypes = [];
        // The collection and its elements are guarded, because the deserializer can produce a null collection, or null elements, regardless of the initializers.
        foreach (string? type in image.Types ?? [])
            if (TryMapArtworkType(type, out ArtworkType artworkType) && !artworkTypes.Contains(artworkType))
                artworkTypes.Add(artworkType);

        // An image that carries no known type still describes the front or the back of the release when it is flagged as such.
        if (artworkTypes.Count == 0)
        {
            if (image.IsFront)
                artworkTypes.Add(ArtworkType.Cover);
            if (image.IsBack)
                artworkTypes.Add(ArtworkType.Back);
        }
        return artworkTypes;
    }

    /// <summary>
    /// Maps a Cover Art Archive image type onto an artwork type.
    /// </summary>
    /// <param name="type">The Cover Art Archive image type.</param>
    /// <param name="artworkType">The mapped artwork type, when the type is recognized.</param>
    /// <returns><see langword="true"/> when the type was recognized, otherwise <see langword="false"/>.</returns>
    private static bool TryMapArtworkType(string? type, out ArtworkType artworkType)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            artworkType = ArtworkType.Other;
            return false;
        }

        switch (type.Trim().ToLowerInvariant())
        {
            case "front":
                artworkType = ArtworkType.Cover;
                return true;
            case "back":
                artworkType = ArtworkType.Back;
                return true;
            case "booklet":
                artworkType = ArtworkType.Booklet;
                return true;
            case "medium":
                artworkType = ArtworkType.Medium;
                return true;
            case "tray":
                artworkType = ArtworkType.Tray;
                return true;
            case "spine":
                artworkType = ArtworkType.Spine;
                return true;
            case "obi":
                artworkType = ArtworkType.Obi;
                return true;
            case "sticker":
                artworkType = ArtworkType.Sticker;
                return true;
            case "poster":
                artworkType = ArtworkType.Poster;
                return true;
            case "liner":
                artworkType = ArtworkType.Liner;
                return true;
            case "watermark":
                artworkType = ArtworkType.Watermark;
                return true;
            default:
                // The Cover Art Archive also uses a free form "Other" type and may add new types over time, which are all kept as generic artwork.
                artworkType = ArtworkType.Other;
                return true;
        }
    }
}
