#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.CoverArtArchive.Core.Api;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.CoverArtArchive.UnitTests.Core.Api;

/// <summary>
/// Contains unit tests for the <see cref="CoverArtArchiveResponseCache"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveResponseCacheTests
{
    private readonly CoverArtArchiveResponseCache _sut = new();

    [Fact]
    public void TryGet_WhenTheKeyWasNeverCached_ShouldReturnFalse()
    {
        // Act
        bool result = _sut.TryGet("missing", out string? json);

        // Assert
        Assert.False(result);
        Assert.Null(json);
    }

    [Fact]
    public void TryGet_WhenTheKeyWasCached_ShouldReturnTheCachedBody()
    {
        // Arrange
        _sut.Set("key", "{\"images\":[]}");

        // Act
        bool result = _sut.TryGet("key", out string? json);

        // Assert
        Assert.True(result);
        Assert.Equal("{\"images\":[]}", json);
    }

    [Fact]
    public void Set_WhenTheKeyAlreadyExists_ShouldReplaceTheCachedBody()
    {
        // Arrange
        _sut.Set("key", "a-longer-value");
        _sut.Set("key", "short");

        // Act
        bool result = _sut.TryGet("key", out string? json);

        // Assert
        Assert.True(result);
        Assert.Equal("short", json);
    }

    [Fact]
    public void Set_WhenTheValueIsTheEmptyNotFoundMarker_ShouldCacheIt()
    {
        // Arrange
        _sut.Set("release/11111111-1111-1111-1111-111111111111", string.Empty);

        // Act
        bool result = _sut.TryGet("release/11111111-1111-1111-1111-111111111111", out string? json);

        // Assert
        Assert.True(result);
        Assert.Equal(string.Empty, json);
    }

    [Fact]
    public void Set_WhenTheValueFitsTheCharacterBudgetExactly_ShouldCacheIt()
    {
        // Arrange
        CoverArtArchiveResponseCache sut = new(maximumEntryCount: 10, maximumCharacterCount: 3, timeToLive: TimeSpan.FromMinutes(1));

        // Act
        sut.Set("exact", "abc");

        // Assert
        Assert.True(sut.TryGet("exact", out string? json));
        Assert.Equal("abc", json);
    }

    [Fact]
    public void Set_WhenTheValueExceedsTheWholeCharacterBudget_ShouldNotCacheIt()
    {
        // Arrange
        CoverArtArchiveResponseCache sut = new(maximumEntryCount: 10, maximumCharacterCount: 3, timeToLive: TimeSpan.FromMinutes(1));

        // Act
        sut.Set("oversized", "abcd");

        // Assert
        Assert.False(sut.TryGet("oversized", out _));
    }

    [Fact]
    public void Set_WhenTheCharacterBudgetIsExceeded_ShouldEvictTheOldestEntryAndKeepTheNewestOne()
    {
        // Arrange
        CoverArtArchiveResponseCache sut = new(maximumEntryCount: 10, maximumCharacterCount: 10, timeToLive: TimeSpan.FromMinutes(1));
        sut.Set("oldest", "1234567890");

        // Act
        sut.Set("newest", "x");

        // Assert
        Assert.False(sut.TryGet("oldest", out _));
        Assert.True(sut.TryGet("newest", out string? json));
        Assert.Equal("x", json);
    }

    [Fact]
    public void Set_WhenTheEntryCountBudgetIsExceeded_ShouldEvictTheOldestEntriesAndKeepTheNewestOne()
    {
        // Arrange
        CoverArtArchiveResponseCache sut = new(maximumEntryCount: 3, maximumCharacterCount: 1_000, timeToLive: TimeSpan.FromMinutes(1));
        sut.Set("entry-1", "1");
        sut.Set("entry-2", "2");
        sut.Set("entry-3", "3");

        // Act
        sut.Set("entry-4", "4");

        // Assert
        // The oldest entries are trimmed to the lower water mark, while the most recently cached one always survives.
        Assert.False(sut.TryGet("entry-1", out _));
        Assert.False(sut.TryGet("entry-2", out _));
        Assert.True(sut.TryGet("entry-3", out _));
        Assert.True(sut.TryGet("entry-4", out _));
    }

    [Fact]
    public void Set_WhenTheEntryCountBudgetIsExceededByOne_ShouldEvictTheOldestEntryAndKeepTheNewOne()
    {
        // Arrange
        CoverArtArchiveResponseCache sut = new(maximumEntryCount: 1, maximumCharacterCount: 1_000, timeToLive: TimeSpan.FromMinutes(1));
        sut.Set("entry-1", "1");

        // Act
        sut.Set("entry-2", "2");

        // Assert
        Assert.True(sut.TryGet("entry-2", out _));
        Assert.False(sut.TryGet("entry-1", out _));
    }

    [Fact]
    public void TryGet_WhenTheCachedEntryExpired_ShouldReturnFalse()
    {
        // Arrange
        CoverArtArchiveResponseCache sut = new(maximumEntryCount: 10, maximumCharacterCount: 1_000, timeToLive: TimeSpan.FromMilliseconds(-1));
        sut.Set("expired", "value");

        // Act
        bool result = sut.TryGet("expired", out string? json);

        // Assert
        Assert.False(result);
        Assert.Null(json);
    }
}
