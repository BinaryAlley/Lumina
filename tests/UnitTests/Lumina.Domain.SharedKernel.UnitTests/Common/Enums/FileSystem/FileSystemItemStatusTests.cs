#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemItemStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemStatusTests
{
    [Fact]
    public void FileSystemItemStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        FileSystemItemStatus[] values = Enum.GetValues<FileSystemItemStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
