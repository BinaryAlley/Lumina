#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.Fixtures.Core.Responses.FileSystemManagement.Common;
using Nemera.Contracts.Responses.FileSystemManagement.Common;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Contracts.UnitTests.Responses.FileSystemManagement.Common;

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
