#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Builds the navigation menu model for the themed navigation template, resolving the authorization conditions and localized labels server side.
/// </summary>
public sealed class ThemeNavBuilder
{
    // the resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Shared._NavMenu";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";

    private readonly IUrlService _urlService;
    private readonly IStringLocalizerFactory _stringLocalizerFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeNavBuilder"/> class.
    /// </summary>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the navigation resources.</param>
    /// <param name="authorizationService">Injected service used to resolve the roles and permissions of the current user.</param>
    /// <param name="httpContextAccessor">Injected accessor for the current HTTP context.</param>
    public ThemeNavBuilder(IUrlService urlService, IStringLocalizerFactory stringLocalizerFactory, IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        _urlService = urlService;
        _stringLocalizerFactory = stringLocalizerFactory;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _localizer = stringLocalizerFactory.Create(VIEW_RESOURCE_BASE_NAME, VIEW_RESOURCE_LOCATION);
    }

    /// <summary>
    /// Builds the navigation menu model for the current user and request.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The navigation menu model, with the sections visible to the current user.</returns>
    public async Task<ThemeNavMenuDto> BuildAsync(CancellationToken cancellationToken)
    {
        ClaimsPrincipal user = _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
        bool isAuthenticated = user.Identity?.IsAuthenticated == true;
        bool isAdmin = isAuthenticated && await _authorizationService.IsInRoleAsync("Admin", cancellationToken).ConfigureAwait(false);
        bool canCreateLibraries = isAuthenticated && (isAdmin || await _authorizationService.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, cancellationToken).ConfigureAwait(false));
        string returnUrl = Uri.EscapeDataString(GetCurrentRequestUrl());
        ThemeNavEntryDto[] languages = CreateLanguageEntries(returnUrl);

        // the mobile and desktop menus arrange the same sections differently, notably the language switcher, which is
        // its own mobile heading but a submenu under Tools on the desktop, so each menu gets its own section list
        List<ThemeNavSectionDto> mobileSections = [];
        mobileSections.Add(CreateSiteSection());
        mobileSections.Add(CreateAccountSection(isAuthenticated));
        if (isAdmin)
            mobileSections.Add(CreateAdministratorSection());
        if (isAuthenticated)
            mobileSections.Add(CreateLibrarySection(canCreateLibraries));
        mobileSections.Add(new ThemeNavSectionDto(Localize("Language"), languages));
        if (isAuthenticated)
            mobileSections.Add(CreateToolsSection(isAuthenticated, languages: []));
        mobileSections.Add(CreateHelpSection());

        List<ThemeNavSectionDto> menubarSections = [];
        menubarSections.Add(CreateSiteSection());
        menubarSections.Add(CreateAccountSection(isAuthenticated));
        if (isAdmin)
            menubarSections.Add(CreateAdministratorSection());
        if (isAuthenticated)
            menubarSections.Add(CreateLibrarySection(canCreateLibraries));
        menubarSections.Add(CreateToolsSection(isAuthenticated, languages));
        menubarSections.Add(CreateHelpSection());

        return new ThemeNavMenuDto("Lumina", mobileSections, menubarSections);
    }

    /// <summary>
    /// Creates the site section, which links to the home page.
    /// </summary>
    /// <returns>The site section of the navigation menu.</returns>
    private ThemeNavSectionDto CreateSiteSection()
    {
        return new ThemeNavSectionDto(
            "Lumina",
            [new ThemeNavEntryDto(Localize("Home"), _urlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED), "nav-link", [])]);
    }

    /// <summary>
    /// Creates the account section, whose entries depend on the authentication state of the user.
    /// </summary>
    /// <param name="isAuthenticated">Whether the current user is authenticated.</param>
    /// <returns>The account section of the navigation menu.</returns>
    private ThemeNavSectionDto CreateAccountSection(bool isAuthenticated)
    {
        List<ThemeNavEntryDto> items = [];
        if (isAuthenticated)
        {
            items.Add(new ThemeNavEntryDto(Localize("Logout"), _urlService.GetAbsoluteUrl(WebRoutes.Authentication.LOGOUT), CssClass: null, Children: []));
            items.Add(new ThemeNavEntryDto(Localize("Register"), _urlService.GetAbsoluteUrl(WebRoutes.Authentication.REGISTER_VIEW), "nav-link", []));
            items.Add(new ThemeNavEntryDto(Localize("ChangePassword"), _urlService.GetAbsoluteUrl(WebRoutes.Authentication.CHANGE_PASSWORD_VIEW), "nav-link", []));
            items.Add(new ThemeNavEntryDto(Localize("Profile"), _urlService.GetAbsoluteUrl(WebRoutes.Authentication.PROFILE_VIEW), "nav-link", []));
        }
        else
        {
            items.Add(new ThemeNavEntryDto(Localize("Login"), _urlService.GetAbsoluteUrl(WebRoutes.Authentication.LOGIN_VIEW), CssClass: null, Children: []));
            items.Add(new ThemeNavEntryDto(Localize("RecoverPassword"), _urlService.GetAbsoluteUrl(WebRoutes.Authentication.RECOVER_PASSWORD_VIEW), CssClass: null, Children: []));
        }

        return new ThemeNavSectionDto(Localize("Account"), items);
    }

    /// <summary>
    /// Creates the administrator section, shown only to administrators.
    /// </summary>
    /// <returns>The administrator section of the navigation menu.</returns>
    private ThemeNavSectionDto CreateAdministratorSection()
    {
        return new ThemeNavSectionDto(
            Localize("Administrator"),
            [
                new ThemeNavEntryDto(Localize("ManageRoles"), _urlService.GetAbsoluteUrl(WebRoutes.Admin.MANAGE_ROLES), "nav-link", []),
                new ThemeNavEntryDto(Localize("ManagePermissions"), _urlService.GetAbsoluteUrl(WebRoutes.Admin.MANAGE_PERMISSIONS), "nav-link", []),
                new ThemeNavEntryDto(Localize("Plugins"), _urlService.GetAbsoluteUrl(WebRoutes.Plugins.INDEX), "nav-link", []),
                new ThemeNavEntryDto(Localize("Themes"), _urlService.GetAbsoluteUrl(WebRoutes.Admin.MANAGE_THEMES), "nav-link", [])
            ]);
    }

    /// <summary>
    /// Creates the library section, shown only to authenticated users.
    /// </summary>
    /// <param name="canCreateLibraries">Whether the current user can create media libraries.</param>
    /// <returns>The library section of the navigation menu.</returns>
    private ThemeNavSectionDto CreateLibrarySection(bool canCreateLibraries)
    {
        List<ThemeNavEntryDto> items =
        [
            new ThemeNavEntryDto(Localize("ManageLibraries"), _urlService.GetAbsoluteUrl(WebRoutes.LibraryManagement.INDEX), "nav-link", [])
        ];
        if (canCreateLibraries)
            items.Add(new ThemeNavEntryDto(Localize("AddNewLibrary"), _urlService.GetAbsoluteUrl(WebRoutes.LibraryManagement.ADD_LIBRARY), "nav-link", []));
        items.Add(new ThemeNavEntryDto(Localize("WrittenContent"), Url: null, CssClass: null,
        [
            new ThemeNavEntryDto(Localize("Books"), "library/written-content-library/books-library/books", "nav-link", []),
            new ThemeNavEntryDto(Localize("Magazines"), "library/written-content/magazines", "nav-link", [])
        ]));
        items.Add(new ThemeNavEntryDto(Localize("Audio"), Url: null, CssClass: null,
        [
            new ThemeNavEntryDto(Localize("Music"), "library/audio/music", "nav-link", []),
            new ThemeNavEntryDto(Localize("Interviews"), "library/audio/interviews", "nav-link", [])
        ]));
        items.Add(new ThemeNavEntryDto(Localize("Video"), Url: null, CssClass: null,
        [
            new ThemeNavEntryDto(Localize("Movies"), "library/video/movies", "nav-link", []),
            new ThemeNavEntryDto(Localize("TvShows"), "library/video/tvshows", "nav-link", [])
        ]));

        return new ThemeNavSectionDto(Localize("Library"), items);
    }

    /// <summary>
    /// Creates the tools section, with the language submenu and the settings entry.
    /// </summary>
    /// <param name="isAuthenticated">Whether the current user is authenticated.</param>
    /// <param name="languages">The language entries of the submenu.</param>
    /// <returns>The tools section of the navigation menu.</returns>
    private ThemeNavSectionDto CreateToolsSection(bool isAuthenticated, IReadOnlyList<ThemeNavEntryDto> languages)
    {
        List<ThemeNavEntryDto> items = [];
        if (languages.Count > 0)
            items.Add(new ThemeNavEntryDto(Localize("Language"), Url: null, CssClass: null, Children: languages));
        if (isAuthenticated)
            items.Add(new ThemeNavEntryDto(Localize("Settings"), _urlService.GetAbsoluteUrl(WebRoutes.Settings.INDEX), "nav-link", []));

        return new ThemeNavSectionDto(Localize("Tools"), items);
    }

    /// <summary>
    /// Creates the help section.
    /// </summary>
    /// <returns>The help section of the navigation menu.</returns>
    private ThemeNavSectionDto CreateHelpSection()
    {
        return new ThemeNavSectionDto(
            Localize("Help"),
            [
                new ThemeNavEntryDto(Localize("Documentation"), "help/documentation", "nav-link", []),
                new ThemeNavEntryDto(Localize("About"), "help/about", "nav-link", [])
            ]);
    }

    /// <summary>
    /// Creates the language entries of the navigation menu, each keeping the current request URL as the return URL.
    /// </summary>
    /// <param name="returnUrl">The encoded current request URL used as the return URL.</param>
    /// <returns>The language entries of the navigation menu.</returns>
    private ThemeNavEntryDto[] CreateLanguageEntries(string returnUrl)
    {
        string baseUrl = _urlService.GetAbsoluteUrl(WebRoutes.Language.SET_LANGUAGE) ?? string.Empty;
        (string Culture, string DisplayName)[] cultures =
        [
            ("zh-cn", "简体中文"),
            ("de-de", "Deutsch"),
            ("en-us", "English"),
            ("es-es", "Español"),
            ("fr-fr", "Français"),
            ("it-it", "Italiano"),
            ("ja-jp", "日本語"),
            ("ro-ro", "Română"),
            ("uk-ua", "Українська")
        ];

        ThemeNavEntryDto[] entries = new ThemeNavEntryDto[cultures.Length];
        for (int index = 0; index < cultures.Length; index++)
        {
            (string culture, string displayName) = cultures[index];
            entries[index] = new ThemeNavEntryDto(displayName, $"{baseUrl}?newCulture={culture}&returnUrl={returnUrl}", "lang-set", []);
        }

        return entries;
    }

    /// <summary>
    /// Gets the path and query string of the current request.
    /// </summary>
    /// <returns>The path and query string of the current request.</returns>
    private string GetCurrentRequestUrl()
    {
        HttpRequest request = _httpContextAccessor.HttpContext?.Request ?? throw new InvalidOperationException("The current HTTP context is unavailable.");
        return $"{request.Path}{request.QueryString}";
    }

    /// <summary>
    /// Resolves a localized navigation label.
    /// </summary>
    /// <param name="key">The resource key of the label.</param>
    /// <returns>The localized label.</returns>
    private string Localize(string key)
    {
        return _localizer[key].Value;
    }
}
