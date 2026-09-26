#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="FileAccessMode"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileAccessModeTests
{
    [Fact]
    public void FileAccessMode_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        FileAccessMode[] values = Enum.GetValues<FileAccessMode>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
