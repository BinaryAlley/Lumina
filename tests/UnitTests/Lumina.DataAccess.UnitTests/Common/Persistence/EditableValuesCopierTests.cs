#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.DataAccess.Common.Persistence;
using Lumina.DataAccess.Core.UoW;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Persistence;

/// <summary>
/// Contains unit tests for the <see cref="EditableValuesCopier"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditableValuesCopierTests
{
    private readonly LuminaDbContext _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
    private readonly ArtistEntityFixture _artistEntityFixture = new();

    [Fact]
    public async Task CopyEditableValues_WhenCalled_ShouldCopyTheEditableScalarValuesOntoTheTrackedEntity()
    {
        // Arrange
        ArtistEntity trackedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        _mockContext.Artists.Add(trackedArtist);
        await _mockContext.SaveChangesAsync();

        ArtistEntity incomingArtist = _artistEntityFixture.Create(id: trackedArtist.Id, name: "Updated Artist", includeAlbums: false, includeContributors: false);
        incomingArtist.Website = "https://updated.example.com";

        // Act
        EditableValuesCopier.CopyEditableValues(_mockContext, trackedArtist, incomingArtist);

        // Assert
        Assert.Equal("Updated Artist", trackedArtist.Name);
        Assert.Equal("https://updated.example.com", trackedArtist.Website);
    }

    [Fact]
    public async Task CopyEditableValues_WhenCalled_ShouldPreserveTheStoredAuditColumns()
    {
        // Arrange
        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        DateTime storedUpdatedOnUtc = new(2021, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        Guid storedUpdatedBy = Guid.NewGuid();
        ArtistEntity trackedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        trackedArtist.CreatedOnUtc = storedCreatedOnUtc;
        trackedArtist.CreatedBy = storedCreatedBy;
        trackedArtist.UpdatedOnUtc = storedUpdatedOnUtc;
        trackedArtist.UpdatedBy = storedUpdatedBy;
        _mockContext.Artists.Add(trackedArtist);
        await _mockContext.SaveChangesAsync();

        ArtistEntity incomingArtist = _artistEntityFixture.Create(id: trackedArtist.Id, includeAlbums: false, includeContributors: false);
        incomingArtist.CreatedOnUtc = DateTime.UtcNow.AddYears(-10);
        incomingArtist.CreatedBy = Guid.NewGuid();
        incomingArtist.UpdatedOnUtc = DateTime.UtcNow;
        incomingArtist.UpdatedBy = Guid.NewGuid();

        // Act
        EditableValuesCopier.CopyEditableValues(_mockContext, trackedArtist, incomingArtist);

        // Assert
        Assert.Equal(storedCreatedOnUtc, trackedArtist.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, trackedArtist.CreatedBy);
        Assert.Equal(storedUpdatedOnUtc, trackedArtist.UpdatedOnUtc);
        Assert.Equal(storedUpdatedBy, trackedArtist.UpdatedBy);
    }

    [Fact]
    public async Task CopyEditableValues_WhenNoEditableValueChanged_ShouldNotMarkAnyEditablePropertyAsModified()
    {
        // Arrange
        ArtistEntity trackedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        _mockContext.Artists.Add(trackedArtist);
        await _mockContext.SaveChangesAsync();

        ArtistEntity incomingArtist = _artistEntityFixture.Create(id: trackedArtist.Id, includeAlbums: false, includeContributors: false);
        incomingArtist.Name = trackedArtist.Name;
        incomingArtist.Website = trackedArtist.Website;
        incomingArtist.MusicBrainzArtistId = trackedArtist.MusicBrainzArtistId;
        incomingArtist.LibraryId = trackedArtist.LibraryId;

        // Act
        EditableValuesCopier.CopyEditableValues(_mockContext, trackedArtist, incomingArtist);

        // Assert
        Assert.False(_mockContext.Entry(trackedArtist).Property(artist => artist.Name).IsModified);
        Assert.False(_mockContext.Entry(trackedArtist).Property(artist => artist.Website).IsModified);
        Assert.False(_mockContext.Entry(trackedArtist).Property(artist => artist.MusicBrainzArtistId).IsModified);
        Assert.False(_mockContext.Entry(trackedArtist).Property(artist => artist.LibraryId).IsModified);
        // The primary key and the audit columns are owned by the persistence medium, so the copy never touches them either.
        Assert.False(_mockContext.Entry(trackedArtist).Property(artist => artist.Id).IsModified);
        Assert.False(_mockContext.Entry(trackedArtist).Property(artist => artist.CreatedOnUtc).IsModified);
        Assert.False(_mockContext.Entry(trackedArtist).Property(artist => artist.CreatedBy).IsModified);
        Assert.False(_mockContext.Entry(trackedArtist).Property(artist => artist.UpdatedOnUtc).IsModified);
        Assert.False(_mockContext.Entry(trackedArtist).Property(artist => artist.UpdatedBy).IsModified);
    }

    [Theory]
    [InlineData(false)] // the incoming entity carries a different, non-empty key
    [InlineData(true)] // the incoming entity carries an unset key
    public async Task CopyEditableValues_WhenTheIncomingKeyDoesNotMatchTheTrackedOne_ShouldPreserveTheTrackedIdentity(bool useUnsetKey)
    {
        // Arrange
        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        ArtistEntity trackedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        trackedArtist.CreatedOnUtc = storedCreatedOnUtc;
        trackedArtist.CreatedBy = storedCreatedBy;
        trackedArtist.UpdatedOnUtc = null;
        trackedArtist.UpdatedBy = null;
        _mockContext.Artists.Add(trackedArtist);
        await _mockContext.SaveChangesAsync();
        Guid trackedArtistId = trackedArtist.Id;

        // The incoming entity carries a key that does not match the tracked one, a changed editable value, and hostile audit values.
        Guid incomingId = useUnsetKey ? Guid.Empty : Guid.NewGuid();
        ArtistEntity incomingArtist = _artistEntityFixture.Create(id: incomingId, name: "Updated Artist", includeAlbums: false, includeContributors: false);
        incomingArtist.CreatedOnUtc = DateTime.UtcNow.AddYears(-10);
        incomingArtist.CreatedBy = Guid.NewGuid();
        incomingArtist.UpdatedOnUtc = DateTime.UtcNow;
        incomingArtist.UpdatedBy = Guid.NewGuid();

        // Act
        EditableValuesCopier.CopyEditableValues(_mockContext, trackedArtist, incomingArtist);

        // Assert
        // The stored identity is never retargeted by the incoming key, the audit columns are untouched, and the editable values are still copied.
        Assert.Equal(trackedArtistId, trackedArtist.Id);
        Assert.NotEqual(incomingId, trackedArtist.Id);
        Assert.Equal("Updated Artist", trackedArtist.Name);
        Assert.Equal(storedCreatedOnUtc, trackedArtist.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, trackedArtist.CreatedBy);
        Assert.Null(trackedArtist.UpdatedOnUtc);
        Assert.Null(trackedArtist.UpdatedBy);
    }
}
