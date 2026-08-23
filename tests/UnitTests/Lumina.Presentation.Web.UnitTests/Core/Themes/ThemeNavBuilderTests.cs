#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeNavBuilder"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeNavBuilderTests
{
    private readonly IUrlService _mockUrlService;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly ThemeNavBuilder _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeNavBuilderTests"/> class.
    /// </summary>
    public ThemeNavBuilderTests()
    {
        _mockUrlService = Substitute.For<IUrlService>();
        _mockStringLocalizer = Substitute.For<IStringLocalizer>();
        _mockStringLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), $"Localized-{callInfo.Arg<string>()}"));
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _sut = new ThemeNavBuilder(_mockUrlService, _mockStringLocalizerFactory, _mockAuthorizationService, _mockHttpContextAccessor);

        StubSiteUrls();
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(false);
        _mockAuthorizationService.HasPermissionAsync(Arg.Any<AuthorizationPermission>(), Arg.Any<CancellationToken>()).Returns(false);
    }

    [Fact]
    public async Task BuildAsync_WhenUserIsAnonymous_ShouldNotIncludeAdministratorLibraryOrToolsSections()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: null));

        // Act
        ThemeNavMenuDto result = await _sut.BuildAsync(CancellationToken.None);

        // Assert
        Assert.Null(FindSection(result, "Localized-Administrator"));
        Assert.Null(FindSection(result, "Localized-Library"));
        Assert.Null(FindSection(result, "Localized-Tools"));
        Assert.NotNull(FindSection(result, "Localized-Language"));
        Assert.NotNull(FindSection(result, "Localized-Help"));
    }

    [Fact]
    public async Task BuildAsync_WhenUserIsAnonymous_ShouldIncludeLoginEntryInsteadOfLogoutEntry()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: null));

        // Act
        ThemeNavMenuDto result = await _sut.BuildAsync(CancellationToken.None);

        // Assert
        ThemeNavSectionDto accountSection = FindSection(result, "Localized-Account")!;
        Assert.Contains(accountSection.Items, entry => entry.Label == "Localized-Login");
        Assert.DoesNotContain(accountSection.Items, entry => entry.Label == "Localized-Logout");
        Assert.Equal("http://localhost/en-us/auth/login", accountSection.Items.First(entry => entry.Label == "Localized-Login").Url);
    }

    [Fact]
    public async Task BuildAsync_WhenUserIsAuthenticated_ShouldIncludeAccountLinksAndLibrarySection()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: TestHttpContextFactory.CreateAuthenticatedUser()));

        // Act
        ThemeNavMenuDto result = await _sut.BuildAsync(CancellationToken.None);

        // Assert
        ThemeNavSectionDto accountSection = FindSection(result, "Localized-Account")!;
        Assert.Contains(accountSection.Items, entry => entry.Label == "Localized-Logout");
        Assert.Contains(accountSection.Items, entry => entry.Label == "Localized-Register");
        Assert.Contains(accountSection.Items, entry => entry.Label == "Localized-ChangePassword");
        Assert.Contains(accountSection.Items, entry => entry.Label == "Localized-Profile");
        Assert.DoesNotContain(accountSection.Items, entry => entry.Label == "Localized-Login");

        ThemeNavSectionDto librarySection = FindSection(result, "Localized-Library")!;
        Assert.Contains(librarySection.Items, entry => entry.Label == "Localized-ManageLibraries");
        Assert.DoesNotContain(librarySection.Items, entry => entry.Label == "Localized-AddNewLibrary");
    }

    [Fact]
    public async Task BuildAsync_WhenUserIsAdmin_ShouldIncludeAdministratorSection()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: TestHttpContextFactory.CreateAuthenticatedUser()));
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(true);

        // Act
        ThemeNavMenuDto result = await _sut.BuildAsync(CancellationToken.None);

        // Assert
        ThemeNavSectionDto administratorSection = FindSection(result, "Localized-Administrator")!;
        Assert.Contains(administratorSection.Items, entry => entry.Label == "Localized-ManageRoles" && entry.Url == "http://localhost/en-us/admin/manage-roles");
        Assert.Contains(administratorSection.Items, entry => entry.Label == "Localized-ManagePermissions" && entry.Url == "http://localhost/en-us/admin/manage-permissions");
        Assert.Contains(administratorSection.Items, entry => entry.Label == "Localized-Plugins" && entry.Url == "http://localhost/en-us/admin/manage-plugins");
        Assert.Contains(administratorSection.Items, entry => entry.Label == "Localized-Themes" && entry.Url == "http://localhost/en-us/admin/manage-themes");
        await _mockAuthorizationService.Received(1).IsInRoleAsync("Admin", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuildAsync_WhenUserCanCreateLibraries_ShouldIncludeAddNewLibraryEntry()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: TestHttpContextFactory.CreateAuthenticatedUser()));
        _mockAuthorizationService.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        ThemeNavMenuDto result = await _sut.BuildAsync(CancellationToken.None);

        // Assert
        ThemeNavSectionDto librarySection = FindSection(result, "Localized-Library")!;
        Assert.Contains(librarySection.Items, entry => entry.Label == "Localized-AddNewLibrary" && entry.Url == "http://localhost/en-us/libraries/manage/item");
        await _mockAuthorizationService.Received(1).HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuildAsync_WhenUserCannotCreateLibraries_ShouldNotIncludeAddNewLibraryEntry()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: TestHttpContextFactory.CreateAuthenticatedUser()));

        // Act
        ThemeNavMenuDto result = await _sut.BuildAsync(CancellationToken.None);

        // Assert
        ThemeNavSectionDto librarySection = FindSection(result, "Localized-Library")!;
        Assert.DoesNotContain(librarySection.Items, entry => entry.Label == "Localized-AddNewLibrary");
    }

    [Fact]
    public async Task BuildAsync_WhenRequestHasPathAndQuery_ShouldKeepEscapedReturnUrlInLanguageEntries()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create(user: null);
        httpContext.Request.Path = "/en-us";
        httpContext.Request.QueryString = new QueryString("?page=1");
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        ThemeNavMenuDto result = await _sut.BuildAsync(CancellationToken.None);

        // Assert
        ThemeNavSectionDto languageSection = FindSection(result, "Localized-Language")!;
        Assert.Equal(10, languageSection.Items.Count);
        Assert.All(languageSection.Items, entry =>
        {
            Assert.Equal("lang-set", entry.CssClass);
            Assert.Contains("returnUrl=%2Fen-us%3Fpage%3D1", entry.Url);
        });
        Assert.Contains(languageSection.Items, entry => entry.Label == "English" && entry.Url!.StartsWith("http://localhost/en-us/tools/language/set-language?newCulture=en-us", StringComparison.Ordinal));
    }

    [Fact]
    public async Task BuildAsync_WhenAuthenticated_ShouldIncludeSettingsEntryInDesktopToolsSection()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: TestHttpContextFactory.CreateAuthenticatedUser()));

        // Act
        ThemeNavMenuDto result = await _sut.BuildAsync(CancellationToken.None);

        // Assert
        ThemeNavSectionDto toolsSection = FindSection(result, "Localized-Tools", isMobile: false)!;
        Assert.Contains(toolsSection.Items, entry => entry.Label == "Localized-Settings" && entry.Url == "http://localhost/en-us/tools/settings");
        // the desktop tools section hosts the language submenu while the mobile menu has a dedicated language section
        Assert.Contains(toolsSection.Items, entry => entry.Label == "Localized-Language" && entry.Children.Count == 10);
    }

    [Fact]
    public async Task BuildAsync_WhenCalled_ShouldCreateLocalizerForTheNavigationResources()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: null));

        // Act
        await _sut.BuildAsync(CancellationToken.None);

        // Assert
        _mockStringLocalizerFactory.Received(1).Create("Lumina.Presentation.Web.Views.Shared._NavMenu", "Lumina.Presentation.Web");
    }

    [Fact]
    public async Task BuildAsync_WhenAdminCreatesLibraries_ShouldAddAddNewLibraryEntryOnce()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: TestHttpContextFactory.CreateAuthenticatedUser()));
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(true);
        _mockAuthorizationService.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        ThemeNavMenuDto result = await _sut.BuildAsync(CancellationToken.None);

        // Assert
        ThemeNavSectionDto librarySection = FindSection(result, "Localized-Library")!;
        Assert.Single(librarySection.Items, entry => entry.Label == "Localized-AddNewLibrary");
    }

    private void StubSiteUrls()
    {
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED).Returns("http://localhost/en-us");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Authentication.LOGIN_VIEW).Returns("http://localhost/en-us/auth/login");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Authentication.RECOVER_PASSWORD_VIEW).Returns("http://localhost/en-us/auth/recover-password");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Authentication.LOGOUT).Returns("http://localhost/en-us/auth/logout");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Authentication.REGISTER_VIEW).Returns("http://localhost/en-us/auth/register");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Authentication.CHANGE_PASSWORD_VIEW).Returns("http://localhost/en-us/auth/change-password");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Authentication.PROFILE_VIEW).Returns("http://localhost/en-us/auth/profile");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Admin.MANAGE_ROLES).Returns("http://localhost/en-us/admin/manage-roles");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Admin.MANAGE_PERMISSIONS).Returns("http://localhost/en-us/admin/manage-permissions");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Plugins.INDEX).Returns("http://localhost/en-us/admin/manage-plugins");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Admin.MANAGE_THEMES).Returns("http://localhost/en-us/admin/manage-themes");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryManagement.INDEX).Returns("http://localhost/en-us/libraries/manage");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.LibraryManagement.ADD_LIBRARY).Returns("http://localhost/en-us/libraries/manage/item");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Settings.INDEX).Returns("http://localhost/en-us/tools/settings");
        _mockUrlService.GetAbsoluteUrl(WebRoutes.Language.SET_LANGUAGE).Returns("http://localhost/en-us/tools/language/set-language");
    }

    private static ThemeNavSectionDto? FindSection(ThemeNavMenuDto menu, string label, bool isMobile = true)
    {
        IReadOnlyList<ThemeNavSectionDto> sections = isMobile ? menu.MobileSections : menu.MenubarSections;
        return sections.FirstOrDefault(section => section.Label == label);
    }
}
