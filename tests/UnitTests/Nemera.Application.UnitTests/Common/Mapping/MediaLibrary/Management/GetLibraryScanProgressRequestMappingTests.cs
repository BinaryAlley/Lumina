#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.MediaLibrary.Management;
using Nemera.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
using Nemera.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Nemera.Contracts.Requests.MediaLibrary.Management;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

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
