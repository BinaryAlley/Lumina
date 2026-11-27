#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="ScanLibraryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryRequestMappingTests
{
    private readonly ScanLibraryRequestFixture _scanLibraryRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        ScanLibraryRequest request = _scanLibraryRequestFixture.Create(id);

        // Act
        ScanLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Equal(request.Id, result.Id);
    }
}
