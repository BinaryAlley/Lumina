#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryScanProgressRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryScanProgressRequestMappingTests
{
    private readonly GetLibraryScanProgressRequestFixture _getLibraryScanProgressRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetLibraryScanProgressRequest request = _getLibraryScanProgressRequestFixture.Create();

        // Act
        GetLibraryScanProgressQuery result = request.ToQuery();

        // Assert
        Assert.Equal(request.LibraryId, result.LibraryId);
        Assert.Equal(request.ScanId, result.ScanId);
    }
}
