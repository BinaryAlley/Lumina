#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="LibraryPathTemplate"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplateTests
{
    private readonly LibraryPathTemplateFixture _libraryPathTemplateFixture = new();

    [Fact]
    public void Create_WhenCalledWithParts_ShouldCreateTemplateWithThoseParts()
    {
        // Arrange
        LibraryPathTemplate template = _libraryPathTemplateFixture.Create();

        // Act
        LibraryPathTemplate result = LibraryPathTemplate.Create(template.Parts);

        // Assert
        Assert.Equal(template.Parts, result.Parts);
        Assert.False(result.IsEmpty);
    }

    [Fact]
    public void Empty_WhenCalled_ShouldCreateTemplateWithoutParts()
    {
        // Act
        LibraryPathTemplate result = LibraryPathTemplate.Empty();

        // Assert
        Assert.Empty(result.Parts);
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void Create_WhenSourceCollectionIsLaterModified_ShouldNotAffectTheTemplate()
    {
        // Arrange
        List<LibraryPathPart> sourceParts = [.. _libraryPathTemplateFixture.Create().Parts];
        int originalCount = sourceParts.Count;

        // Act
        LibraryPathTemplate template = LibraryPathTemplate.Create(sourceParts);
        sourceParts.Clear();

        // Assert
        Assert.Equal(originalCount, template.Parts.Count);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        LibraryPathTemplate firstTemplate = _libraryPathTemplateFixture.Create();

        // Act
        LibraryPathTemplate secondTemplate = LibraryPathTemplate.Create(firstTemplate.Parts);

        // Assert
        Assert.Equal(firstTemplate, secondTemplate);
    }

    [Fact]
    public void Equals_WithDifferentParts_ShouldReturnFalse()
    {
        // Arrange
        LibraryPathTemplate firstTemplate = _libraryPathTemplateFixture.Create();

        // Act
        LibraryPathTemplate secondTemplate = _libraryPathTemplateFixture.Create();

        // Assert
        Assert.NotEqual(firstTemplate, secondTemplate);
    }

    [Fact]
    public void Equals_WhenBothTemplatesAreEmpty_ShouldReturnTrue()
    {
        // Act
        LibraryPathTemplate firstTemplate = LibraryPathTemplate.Empty();
        LibraryPathTemplate secondTemplate = LibraryPathTemplate.Empty();

        // Assert
        Assert.Equal(firstTemplate, secondTemplate);
    }
}
