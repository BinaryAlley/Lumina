#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.ValidatePath;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.Requests.FileSystemManagement.Path;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.FileSystemManagement.Path.ValidatePath;

/// <summary>
/// Contains unit tests for the <see cref="ValidatePathEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly ValidatePathEndpoint _sut;
    private readonly ValidatePathRequestFixture _validatePathRequestFixture = new();
    private readonly PathValidDtoFixture _pathValidDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpointTests"/> class.
    /// </summary>
    public ValidatePathEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<ValidatePathEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiValidatesPath_ShouldReturnSuccessJsonWithIsValid()
    {
        // Arrange
        ValidatePathRequest request = _validatePathRequestFixture.Create(path: "/media/books");
        _mockApiHttpClient.GetAsync<PathValidDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_pathValidDtoFixture.Create(isValid: true));

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        string expectedEndpoint = $"{ApiRoutes.Path.VALIDATE}?path={Uri.EscapeDataString(request.Path!)}";
        await _mockApiHttpClient.Received(1).GetAsync<PathValidDto>(expectedEndpoint, Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.True(jsonDocument.RootElement.GetProperty("data").GetProperty("isValid").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPathIsInvalid_ShouldReturnSuccessJsonWithIsValidFalse()
    {
        // Arrange
        ValidatePathRequest request = _validatePathRequestFixture.Create(path: "C:/invalid|path");
        _mockApiHttpClient.GetAsync<PathValidDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_pathValidDtoFixture.Create(isValid: false));

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.False(jsonDocument.RootElement.GetProperty("data").GetProperty("isValid").GetBoolean());
    }
}
