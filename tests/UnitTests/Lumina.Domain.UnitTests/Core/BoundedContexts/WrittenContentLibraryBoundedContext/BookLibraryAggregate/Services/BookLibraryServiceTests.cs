#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="BookLibraryService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookLibraryServiceTests
{
    [Fact]
    public void Books_WhenAccessed_ShouldThrowNotImplementedException()
    {
        // Arrange
        BookLibraryService sut = new();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => sut.Books);
    }
}
