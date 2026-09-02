#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.UsersManagement.Settings;

/// <summary>
/// Represents the settings of a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user that owns these settings.</param>
/// <param name="IsPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
/// <param name="ItemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
/// <param name="ShouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
/// <param name="IsThemeCachingEnabled">Whether the theme data served to this user is cached, or not.</param>
/// <param name="ShouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.</param>
/// <param name="ShouldRenderPdfAsImages">Whether PDF books are rendered as page images for the user, or not.</param>
/// <param name="ShouldPreserveBookStyles">Whether the styles of the book content are preserved when it is rendered for the user, or not.</param>
[DebuggerDisplay("UserId: {UserId}")]
public sealed record UserSettingsResponse(
    Guid UserId,
    bool IsPaginationEnabled,
    int ItemsPerPage,
    bool ShouldIgnoreThePrefixForAlphaPicker,
    bool IsThemeCachingEnabled,
    bool ShouldAggregateMetadataWhenMissing,
    bool ShouldRenderPdfAsImages,
    bool ShouldPreserveBookStyles
);
