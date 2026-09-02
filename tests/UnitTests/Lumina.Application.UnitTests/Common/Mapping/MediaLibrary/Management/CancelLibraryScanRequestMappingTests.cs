#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="CancelLibraryScanRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibraryScanRequestMappingTests
{
    private readonly CancelLibraryScanRequestFixture _cancelLibraryScanRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        CancelLibraryScanRequest request = _cancelLibraryScanRequestFixture.Create();

        // Act
        CancelLibraryScanCommand result = request.ToCommand();

        // Assert
        Assert.Equal(request.LibraryId, result.LibraryId);
        Assert.Equal(request.ScanId, result.ScanId);
    }
}
