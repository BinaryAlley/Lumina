#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraryPathTemplateParts;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Management;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Management.GetLibraryPathTemplateParts;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryPathTemplatePartsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryPathTemplatePartsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetLibraryPathTemplatePartsEndpoint _sut;
    private readonly GetLibraryPathTemplatePartsRequestFixture _requestFixture = new();
    private readonly LibraryPathTemplateCatalogDtoFixture _catalogDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryPathTemplatePartsEndpointTests"/> class.
    /// </summary>
    public GetLibraryPathTemplatePartsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetLibraryPathTemplatePartsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnSuccessJsonWithCatalog()
    {
        // Arrange
        GetLibraryPathTemplatePartsRequest request = _requestFixture.Create();
        LibraryPathTemplateCatalogDto catalog = _catalogDtoFixture.Create();
        _mockApiHttpClient.GetAsync<LibraryPathTemplateCatalogDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(catalog);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());

        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        JsonElement parts = data.GetProperty("parts");
        JsonElement defaultTemplateParts = data.GetProperty("defaultTemplateParts");
        Assert.Equal(catalog.Parts.Count, parts.GetArrayLength());
        Assert.Equal(catalog.DefaultTemplateParts.Count, defaultTemplateParts.GetArrayLength());

        JsonElement firstPart = parts[0];
        Assert.Equal(catalog.Parts[0].Kind, firstPart.GetProperty("kind").GetString());
        Assert.Equal(catalog.Parts[0].ValueType, firstPart.GetProperty("valueType").GetString());
        Assert.Equal(catalog.Parts[0].DefaultRepresentation, firstPart.GetProperty("defaultRepresentation").GetString());
        Assert.Equal(catalog.Parts[0].IsOptionalByDefault, firstPart.GetProperty("isOptionalByDefault").GetBoolean());

        JsonElement firstDefaultTemplatePart = defaultTemplateParts[0];
        Assert.Equal(catalog.DefaultTemplateParts[0].Kind, firstDefaultTemplatePart.GetProperty("kind").GetString());
        Assert.Equal(catalog.DefaultTemplateParts[0].Representation, firstDefaultTemplatePart.GetProperty("representation").GetString());
        Assert.Equal(catalog.DefaultTemplateParts[0].IsOptional, firstDefaultTemplatePart.GetProperty("isOptional").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestCatalogFromApiWithSubstitutedLibraryType()
    {
        // Arrange
        GetLibraryPathTemplatePartsRequest request = _requestFixture.Create(libraryType: "Music");
        LibraryPathTemplateCatalogDto catalog = _catalogDtoFixture.Create();
        _mockApiHttpClient.GetAsync<LibraryPathTemplateCatalogDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(catalog);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryPathTemplateCatalogDto>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Libraries.GET_LIBRARY_PATH_TEMPLATE_PARTS.Replace("{libraryType}", "Music")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardCancellationTokenToApi()
    {
        // Arrange
        GetLibraryPathTemplatePartsRequest request = _requestFixture.Create();
        LibraryPathTemplateCatalogDto catalog = _catalogDtoFixture.Create();
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        _mockApiHttpClient.GetAsync<LibraryPathTemplateCatalogDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(catalog);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<LibraryPathTemplateCatalogDto>(
            Arg.Any<string>(),
            Arg.Is(cancellationToken));
    }
}
