#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Builds the file system browser model for the themed component template, resolving the localized strings and theme asset URLs server side.
/// </summary>
public sealed class ThemeFileSystemBrowserBuilder
{
    // the resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Shared.Components.FileSystemBrowser.Default";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeFileSystemBrowserBuilder"/> class.
    /// </summary>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the file system browser resources.</param>
    /// <param name="httpContextAccessor">Injected accessor for the current HTTP context.</param>
    public ThemeFileSystemBrowserBuilder(IStringLocalizerFactory stringLocalizerFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _localizer = stringLocalizerFactory.Create(VIEW_RESOURCE_BASE_NAME, VIEW_RESOURCE_LOCATION);
    }

    /// <summary>
    /// Builds the model of the file system browser component for the given theme.
    /// </summary>
    /// <param name="assetBase">The base URL of the assets of the active theme.</param>
    /// <param name="treeNodeTemplate">The raw template source of the tree view node, or an empty string when the theme does not provide one.</param>
    /// <param name="explorerItemTemplate">The raw template source of the explorer item, or an empty string when the theme does not provide one.</param>
    /// <param name="pathSegmentTemplate">The raw template source of the address bar path segment, or an empty string when the theme does not provide one.</param>
    /// <returns>The model available to the themed file system browser template.</returns>
    public ThemeFileSystemBrowserDto Build(
        string assetBase,
        string treeNodeTemplate,
        string explorerItemTemplate,
        string pathSegmentTemplate)
    {
        Dictionary<string, object?> strings = new()
        {
            ["listView"] = Localize("ListView"),
            ["detailsView"] = Localize("DetailsView"),
            ["smallIconsView"] = Localize("SmallIconsView"),
            ["mediumIconsView"] = Localize("MediumIconsView"),
            ["largeIconsView"] = Localize("LargeIconsView"),
            ["extraLargeIconsView"] = Localize("ExtraLargeIconsView"),
            ["back"] = Localize("Back"),
            ["forward"] = Localize("Forward"),
            ["upOneLevel"] = Localize("UpOneLevel"),
            ["toggleTreeView"] = Localize("ToggleTreeView"),
            ["toggleThumbnails"] = Localize("ToggleThumbanils"),
            ["toggleHiddenItems"] = Localize("ToggleHiddenItems"),
            ["toggleSelectionMode"] = Localize("ToggleSelectionMode"),
            ["editPath"] = Localize("EditPath"),
            ["navigate"] = Localize("Navigate"),
            ["newDirectory"] = Localize("NewDirectory"),
            ["favoriteDirectory"] = Localize("FavoriteDirectory"),
            ["name"] = Localize("Name"),
            ["directory"] = Localize("Directory"),
            ["cancel"] = Localize("Cancel"),
            ["open"] = Localize("Open")
        };

        return new ThemeFileSystemBrowserDto(
            AppBase: GetAppBaseUrl(),
            AssetBase: assetBase,
            IconBaseUrl: $"{assetBase}/images/icons",
            FileIconsUrl: $"{assetBase}/file-icons.json",
            TreeNodeTemplate: treeNodeTemplate,
            ExplorerItemTemplate: explorerItemTemplate,
            PathSegmentTemplate: pathSegmentTemplate,
            Strings: strings);
    }

    /// <summary>
    /// Gets the absolute base URL of the current Web application request, used to reference the application scripts from a static theme template.
    /// </summary>
    /// <returns>The absolute base URL with a trailing slash, or an empty string when the current HTTP context is unavailable.</returns>
    private string GetAppBaseUrl()
    {
        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return string.Empty;

        return $"{request.Scheme}://{request.Host}{request.PathBase}/";
    }

    /// <summary>
    /// Resolves a localized file system browser string.
    /// </summary>
    /// <param name="key">The resource key of the string.</param>
    /// <returns>The localized string.</returns>
    private string Localize(string key)
    {
        return _localizer[key].Value;
    }
}
