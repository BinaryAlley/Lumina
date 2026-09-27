#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.FileSystemManagement.Common;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemTreeNodeResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemTreeNodeResponseTests
{
    [Fact]
    public void Constructor_WhenInstantiatingNode_ShouldInitializeEmptyChildren()
    {
        // Act
        FileSystemTreeNodeResponse sut = new();

        // Assert
        Assert.NotNull(sut.Children);
        Assert.Empty(sut.Children);
    }
}
