#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Common;
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
    private readonly FileSystemTreeNodeResponseFixture _fileSystemTreeNodeResponseFixture = new();

    [Fact]
    public void Constructor_WhenInstantiatingNode_ShouldInitializeEmptyChildren()
    {
        // Act
        FileSystemTreeNodeResponse sut = _fileSystemTreeNodeResponseFixture.Create(maxDepth: 0);

        // Assert
        Assert.NotNull(sut.Children);
        Assert.Empty(sut.Children);
    }
}
