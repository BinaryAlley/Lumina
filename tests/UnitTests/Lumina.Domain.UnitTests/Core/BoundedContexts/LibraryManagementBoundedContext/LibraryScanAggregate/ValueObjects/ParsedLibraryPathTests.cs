#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="ParsedLibraryPath"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ParsedLibraryPathTests
{
    private readonly ParsedLibraryPathFixture _parsedLibraryPathFixture = new();

    [Fact]
    public void Create_WhenCalledWithValues_ShouldExposeThoseValues()
    {
        // Arrange
        Dictionary<LibraryPathPartKind, string> values = new()
        {
            [LibraryPathPartKind.Artist] = "Pink Floyd",
            [LibraryPathPartKind.TrackName] = "Time"
        };

        // Act
        ParsedLibraryPath result = ParsedLibraryPath.Create(values);

        // Assert
        Assert.Equal(2, result.Values.Count);
        Assert.Equal("Pink Floyd", result.Values[LibraryPathPartKind.Artist]);
        Assert.Equal("Time", result.Values[LibraryPathPartKind.TrackName]);
    }

    [Fact]
    public void GetString_WhenKindIsCaptured_ShouldReturnTheTrimmedValue()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "  Pink Floyd  "
        });

        // Act
        Optional<string> result = parsedPath.GetString(LibraryPathPartKind.Artist);

        // Assert
        Assert.True(result.HasValue);
        Assert.Equal("Pink Floyd", result.Value);
    }

    [Fact]
    public void GetString_WhenKindIsNotCaptured_ShouldReturnNoValue()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "Pink Floyd"
        });

        // Act
        Optional<string> result = parsedPath.GetString(LibraryPathPartKind.TrackName);

        // Assert
        Assert.False(result.HasValue);
    }

    [Fact]
    public void GetString_WhenCapturedValueIsWhitespace_ShouldReturnNoValue()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "   "
        });

        // Act
        Optional<string> result = parsedPath.GetString(LibraryPathPartKind.Artist);

        // Assert
        Assert.False(result.HasValue);
    }

    [Fact]
    public void GetInt_WhenKindIsCapturedWithAnInteger_ShouldReturnTheValue()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.ReleaseYear] = "1973"
        });

        // Act
        Optional<int> result = parsedPath.GetInt(LibraryPathPartKind.ReleaseYear);

        // Assert
        Assert.True(result.HasValue);
        Assert.Equal(1973, result.Value);
    }

    [Fact]
    public void GetInt_WhenKindIsNotCaptured_ShouldReturnNoValue()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "Pink Floyd"
        });

        // Act
        Optional<int> result = parsedPath.GetInt(LibraryPathPartKind.ReleaseYear);

        // Assert
        Assert.False(result.HasValue);
    }

    [Fact]
    public void GetInt_WhenCapturedValueIsNotAnInteger_ShouldReturnNoValue()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.ReleaseYear] = "not a year"
        });

        // Act
        Optional<int> result = parsedPath.GetInt(LibraryPathPartKind.ReleaseYear);

        // Assert
        Assert.False(result.HasValue);
    }

    [Fact]
    public void ToDictionary_WhenCalled_ShouldReturnAllCapturedValues()
    {
        // Arrange
        Dictionary<LibraryPathPartKind, string> values = new()
        {
            [LibraryPathPartKind.Artist] = "Pink Floyd",
            [LibraryPathPartKind.TrackName] = "Time"
        };
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(values);

        // Act
        IReadOnlyDictionary<LibraryPathPartKind, string> result = parsedPath.ToDictionary();

        // Assert
        Assert.Equal(values, result);
    }

    [Fact]
    public void ToDictionary_WhenCalled_ShouldReturnACopy()
    {
        // Arrange
        Dictionary<LibraryPathPartKind, string> values = new()
        {
            [LibraryPathPartKind.Artist] = "Pink Floyd"
        };
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(values);

        // Act
        IReadOnlyDictionary<LibraryPathPartKind, string> result = parsedPath.ToDictionary();

        // Assert
        Assert.NotSame(values, result);
    }

    [Fact]
    public void Equals_WithSameValuesInDifferentOrder_ShouldReturnTrue()
    {
        // Arrange
        ParsedLibraryPath firstPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "Pink Floyd",
            [LibraryPathPartKind.TrackName] = "Time"
        });

        // Act
        ParsedLibraryPath secondPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.TrackName] = "Time",
            [LibraryPathPartKind.Artist] = "Pink Floyd"
        });

        // Assert
        Assert.Equal(firstPath, secondPath);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        ParsedLibraryPath firstPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "Pink Floyd"
        });

        // Act
        ParsedLibraryPath secondPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "Queen"
        });

        // Assert
        Assert.NotEqual(firstPath, secondPath);
    }

    [Fact]
    public void Equals_WithDifferentKeys_ShouldReturnFalse()
    {
        // Arrange
        ParsedLibraryPath firstPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "Pink Floyd"
        });

        // Act
        ParsedLibraryPath secondPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Title] = "Pink Floyd"
        });

        // Assert
        Assert.NotEqual(firstPath, secondPath);
    }
}
