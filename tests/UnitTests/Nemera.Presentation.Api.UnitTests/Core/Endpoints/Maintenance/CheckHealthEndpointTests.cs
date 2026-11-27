#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Api.Core.Endpoints.Maintenance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Maintenance;

/// <summary>
/// Contains unit tests for the <see cref="CheckHealthEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckHealthEndpointTests
{
    private readonly CheckHealthEndpoint _sut = Factory.Create<CheckHealthEndpoint>();

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.IsType<Ok>(result);
    }
}
