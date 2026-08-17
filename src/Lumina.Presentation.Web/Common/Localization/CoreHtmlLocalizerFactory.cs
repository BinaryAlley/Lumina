#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Localization;

/// <summary>
/// <see cref="IHtmlLocalizerFactory"/> that resolves the resources of the views that live under the <c>Core/Views</c> directory.
/// </summary>
/// <remarks>
/// The view localizer derives the resource base name from the executing view path, so a view at <c>/Core/Views/Auth/Login.cshtml</c> produces the base name
/// <c>{ApplicationName}.Core.Views.Auth.Login</c>. Because the <c>Core</c> segment is only the directory that hosts the views and not part of the resource path,
/// it is stripped here, so that the resources are looked up at <c>{ApplicationName}.Core.Resources.Views.Auth.Login</c>, matching the resource files under
/// <c>Core/Resources/Views/Auth</c>.
/// </remarks>
public class CoreHtmlLocalizerFactory : IHtmlLocalizerFactory
{
    private readonly HtmlLocalizerFactory _innerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreHtmlLocalizerFactory"/> class.
    /// </summary>
    /// <param name="stringLocalizerFactory">Injected factory used to create the underlying string localizers.</param>
    public CoreHtmlLocalizerFactory(IStringLocalizerFactory stringLocalizerFactory)
    {
        _innerFactory = new HtmlLocalizerFactory(stringLocalizerFactory);
    }

    /// <summary>
    /// Creates an <see cref="IHtmlLocalizer"/> for the specified base name and location.
    /// </summary>
    /// <param name="baseName">The base name of the resource to load strings from.</param>
    /// <param name="location">The location to load resources from.</param>
    /// <returns>The created <see cref="IHtmlLocalizer"/>.</returns>
    public IHtmlLocalizer Create(string baseName, string location)
    {
        string prefix = location + ".Core.";
        string resourceBaseName = baseName.StartsWith(prefix, StringComparison.Ordinal)
            ? location + "." + baseName.Substring(prefix.Length)
            : baseName;
        return _innerFactory.Create(resourceBaseName, location);
    }

    /// <summary>
    /// Creates an <see cref="IHtmlLocalizer"/> for the specified resource source type.
    /// </summary>
    /// <param name="resourceSource">The type of the resource to load strings from.</param>
    /// <returns>The created <see cref="IHtmlLocalizer"/>.</returns>
    public IHtmlLocalizer Create(Type resourceSource)
    {
        return _innerFactory.Create(resourceSource);
    }
}
