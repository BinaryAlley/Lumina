#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="BookReaderEnablementCache"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookReaderEnablementCacheTests
{
    private readonly BookReaderEnablementCache _sut = new();

    [Fact]
    public void Get_WhenNothingIsCached_ShouldReturnNull()
    {
        // Act
        bool? result = _sut.Get(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(true)] // reader enabled
    [InlineData(false)] // reader disabled
    public void Set_WhenCalled_ThenGetShouldReturnTheCachedValue(bool isEnabled)
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();

        // Act
        _sut.Set(libraryId, pluginId, isEnabled);

        // Assert
        Assert.Equal(isEnabled, _sut.Get(libraryId, pluginId));
    }

    [Fact]
    public void Invalidate_WhenCalled_ShouldRemoveOnlyTheGivenLibraryAndPluginPair()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid firstPluginId = Guid.NewGuid();
        Guid secondPluginId = Guid.NewGuid();
        _sut.Set(libraryId, firstPluginId, isEnabled: true);
        _sut.Set(libraryId, secondPluginId, isEnabled: false);

        // Act
        _sut.Invalidate(libraryId, firstPluginId);

        // Assert
        Assert.Null(_sut.Get(libraryId, firstPluginId));
        Assert.False(_sut.Get(libraryId, secondPluginId));
    }

    [Fact]
    public void InvalidateLibrary_WhenCalled_ShouldRemoveEveryEntryOfTheLibraryAndKeepTheOthers()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid otherLibraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        Guid otherPluginId = Guid.NewGuid();
        _sut.Set(libraryId, pluginId, isEnabled: true);
        _sut.Set(libraryId, otherPluginId, isEnabled: false);
        _sut.Set(otherLibraryId, pluginId, isEnabled: true);

        // Act
        _sut.InvalidateLibrary(libraryId);

        // Assert
        Assert.Null(_sut.Get(libraryId, pluginId));
        Assert.Null(_sut.Get(libraryId, otherPluginId));
        Assert.True(_sut.Get(otherLibraryId, pluginId));
    }

    [Fact]
    public void InvalidatePlugin_WhenCalled_ShouldRemoveEveryEntryOfThePluginAndKeepTheOthers()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid otherLibraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        Guid otherPluginId = Guid.NewGuid();
        _sut.Set(libraryId, pluginId, isEnabled: true);
        _sut.Set(otherLibraryId, pluginId, isEnabled: false);
        _sut.Set(libraryId, otherPluginId, isEnabled: true);

        // Act
        _sut.InvalidatePlugin(pluginId);

        // Assert
        Assert.Null(_sut.Get(libraryId, pluginId));
        Assert.Null(_sut.Get(otherLibraryId, pluginId));
        Assert.True(_sut.Get(libraryId, otherPluginId));
    }
}
