#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Domain.SharedKernel.UnitTests.Common.Enums.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemItemType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemTypeTests
{
    [Fact]
    public void FileSystemItemType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        FileSystemItemType[] values = Enum.GetValues<FileSystemItemType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
