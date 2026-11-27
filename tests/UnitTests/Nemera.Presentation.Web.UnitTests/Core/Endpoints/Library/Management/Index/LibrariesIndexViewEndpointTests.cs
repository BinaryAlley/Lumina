#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.Index;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Management.Index;

/// <summary>
/// Contains unit tests for the <see cref="LibrariesIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibrariesIndexViewEndpointTests
{
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly LibrariesIndexViewEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibrariesIndexViewEndpointTests"/> class.
    /// </summary>
    public LibrariesIndexViewEndpointTests()
    {
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _sut = Factory.Create<LibrariesIndexViewEndpoint>(_mockAuthorizationService);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
        _sut.HttpContext.Request.Path = "/en-us/libraries/manage";
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheUserHoldsTheManagePermission_ShouldRenderTheIndexViewWithManagementAllowed()
    {
        // Arrange
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(false);
        _mockAuthorizationService.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Library/Management/Index.cshtml", GetRenderedViewName(result));
        LibrariesIndexPageDto model = Assert.IsType<LibrariesIndexPageDto>(GetRenderedModel(result));
        Assert.True(model.CanManageLibraries);
        await _mockAuthorizationService.Received(1).IsInRoleAsync("Admin", Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheUserIsAnAdministratorWithoutTheManagePermission_ShouldRenderTheIndexViewWithManagementAllowed()
    {
        // Arrange
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(true);
        _mockAuthorizationService.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Library/Management/Index.cshtml", GetRenderedViewName(result));
        LibrariesIndexPageDto model = Assert.IsType<LibrariesIndexPageDto>(GetRenderedModel(result));
        Assert.True(model.CanManageLibraries);
        await _mockAuthorizationService.DidNotReceive().HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheUserIsNeitherAnAdministratorNorHoldsTheManagePermission_ShouldRenderTheIndexViewWithManagementDenied()
    {
        // Arrange
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(false);
        _mockAuthorizationService.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("/Core/Views/Library/Management/Index.cshtml", GetRenderedViewName(result));
        LibrariesIndexPageDto model = Assert.IsType<LibrariesIndexPageDto>(GetRenderedModel(result));
        Assert.False(model.CanManageLibraries);
    }

    /// <summary>
    /// Reads the private view name of the rendered <see cref="RazorViewResult"/>, which the result does not expose publicly.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The name or path of the rendered view.</returns>
    private static string GetRenderedViewName(IResult result)
    {
        RazorViewResult viewResult = Assert.IsType<RazorViewResult>(result);
        FieldInfo? viewNameField = typeof(RazorViewResult).GetField("_viewName", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(viewNameField);
        return (string)viewNameField.GetValue(viewResult)!;
    }

    /// <summary>
    /// Reads the private model of the rendered <see cref="RazorViewResult"/>, which the result does not expose publicly.
    /// </summary>
    /// <param name="result">The result returned by the endpoint under test.</param>
    /// <returns>The model passed to the rendered view.</returns>
    private static object? GetRenderedModel(IResult result)
    {
        RazorViewResult viewResult = Assert.IsType<RazorViewResult>(result);
        FieldInfo? modelField = typeof(RazorViewResult).GetField("_model", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(modelField);
        return modelField.GetValue(viewResult);
    }
}
