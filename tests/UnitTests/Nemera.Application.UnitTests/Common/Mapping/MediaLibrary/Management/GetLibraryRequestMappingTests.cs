#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.MediaLibrary.Management;
using Nemera.Application.Core.MediaLibrary.Management.Queries.GetLibrary;
using Nemera.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Nemera.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

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
