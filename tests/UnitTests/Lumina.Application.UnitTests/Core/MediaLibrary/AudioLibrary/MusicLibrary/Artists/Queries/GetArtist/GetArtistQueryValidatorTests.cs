#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;

/// <summary>
/// Contains unit tests for the <see cref="GetArtistQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistQueryValidatorTests
{
    private readonly GetArtistQueryFixture _getArtistQueryFixture = new();
    private readonly GetArtistQueryValidator _validator = new();

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-a-library-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string? libraryId)
    {
        // Arrange
        GetArtistQuery query = _getArtistQueryFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-an-artist-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenArtistIdIsEmptyOrInvalid_ShouldHaveValidationError(string? artistId)
    {
        // Arrange
        GetArtistQuery query = _getArtistQueryFixture.Create(artistId: artistId, includeArtistId: artistId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetArtistQuery query = _getArtistQueryFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetArtistQuery query = _getArtistQueryFixture.Create(artistId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        GetArtistQuery query = _getArtistQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
