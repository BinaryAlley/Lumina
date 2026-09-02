#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryRequestMappingTests
{
    private readonly GetLibraryRequestFixture _getLibraryRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        GetLibraryRequest request = _getLibraryRequestFixture.Create(id);

        // Act
        GetLibraryQuery result = request.ToQuery();

        // Assert
        Assert.Equal(request.Id, result.Id);
    }
}
