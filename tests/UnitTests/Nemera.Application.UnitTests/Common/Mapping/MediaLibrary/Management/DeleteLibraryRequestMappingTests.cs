#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.DeleteLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="DeleteLibraryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteLibraryRequestMappingTests
{
    private readonly DeleteLibraryRequestFixture _deleteLibraryRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        DeleteLibraryRequest request = _deleteLibraryRequestFixture.Create(id);

        // Act
        DeleteLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Equal(request.Id, result.Id);
    }
}
