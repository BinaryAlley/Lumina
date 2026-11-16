#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System.Diagnostics.CodeAnalysis;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="FavoriteMediaKindExtensions"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteMediaKindExtensionsTests
{
    [Theory]
    [InlineData(FavoriteMediaKind.MusicArtist, nameof(DomainErrors.Music.ArtistNotFound))] // An artist has no library of its own.
    [InlineData(FavoriteMediaKind.MusicAlbum, nameof(DomainErrors.Music.AlbumNotFound))] // An album is not a media item that can be favorited.
    [InlineData(FavoriteMediaKind.MusicTrack, nameof(DomainErrors.Music.TrackNotFound))] // A track is resolved through the music catalogue.
    [InlineData(FavoriteMediaKind.Book, nameof(DomainErrors.WrittenContent.BookNotFound))] // A book is resolved through the written content catalogue.
    public void ToMediaItemNotFoundError_WhenTheMediaKindIsCatalogued_ShouldReturnTheMediaSpecificNotFoundError(FavoriteMediaKind mediaKind, string expectedDescription)
    {
        // Act
        Error error = mediaKind.ToMediaItemNotFoundError();

        // Assert
        Assert.Equal(expectedDescription, error.Description);
    }

    [Theory]
    [InlineData(FavoriteMediaKind.None)] // The default value, which is not a kind that can be favorited.
    [InlineData(FavoriteMediaKind.Video)] // Video libraries are not catalogued yet.
    [InlineData(FavoriteMediaKind.Photo)] // Photo libraries are not catalogued yet.
    public void ToMediaItemNotFoundError_WhenTheMediaKindIsNotCatalogued_ShouldReturnTheNotSupportedError(FavoriteMediaKind mediaKind)
    {
        // Act
        Error error = mediaKind.ToMediaItemNotFoundError();

        // Assert
        Assert.Equal(DomainErrors.Favorites.FavoriteMediaKindIsNotSupported, error);
    }

    [Fact]
    public void ToMediaItemNotFoundError_WhenTheMediaKindIsUndefined_ShouldReturnTheNotSupportedError()
    {
        // Arrange
        FavoriteMediaKind mediaKind = (FavoriteMediaKind)int.MaxValue;

        // Act
        Error error = mediaKind.ToMediaItemNotFoundError();

        // Assert
        Assert.Equal(DomainErrors.Favorites.FavoriteMediaKindIsNotSupported, error);
    }
}
