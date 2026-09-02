#region ========================================================================= USING =====================================================================================
using Ganss.Xss;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Sanitizes the HTML content of the reading sections extracted from the books, stripping every active content and keeping only the inert markup a reader can render.
/// The sanitization is generic and is always applied by the host, regardless of what the reader plugin produced.
/// </summary>
public static class ReadingContentSanitizer
{
    public const string RESOURCE_ATTRIBUTE = "data-lumina-resource";

    // The sanitizer that preserves the style attribute, used for the users who want the books to keep their original look, and the
    // sanitizer that strips it, used for the users who prefer a book to never be able to load resources through its styles.
    // The two are configured once at startup, because the allowlist of a sanitizer must not be mutated while it is shared between requests.
    private static readonly HtmlSanitizer s_sanitizerWithStyles = CreateSanitizer(shouldPreserveStyles: true);
    private static readonly HtmlSanitizer s_sanitizerWithoutStyles = CreateSanitizer(shouldPreserveStyles: false);

    /// <summary>
    /// Sanitizes the provided <paramref name="html"/>, returning the safe HTML content ready to be rendered by the client.
    /// </summary>
    /// <param name="html">The HTML content to sanitize.</param>
    /// <param name="shouldPreserveStyles">Whether the style attributes of the content are preserved, or stripped.</param>
    /// <returns>The sanitized HTML content.</returns>
    public static string Sanitize(string html, bool shouldPreserveStyles)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;
        HtmlSanitizer sanitizer = shouldPreserveStyles ? s_sanitizerWithStyles : s_sanitizerWithoutStyles;
        return sanitizer.Sanitize(html);
    }

    /// <summary>
    /// Creates the sanitizer configured with the tags, attributes, and schemes a reading section may use.
    /// </summary>
    /// <param name="shouldPreserveStyles">Whether the style attribute is included in the allowlist of the sanitizer, or not.</param>
    /// <returns>The configured sanitizer.</returns>
    private static HtmlSanitizer CreateSanitizer(bool shouldPreserveStyles)
    {
        // The sanitizer uses an explicit allowlist of the inert markup a book may use, so that anything else - scripts, iframes, objects, forms, SVG, event handlers, and external references
        // is stripped by default, instead of having to enumerate every active construct to remove.
        HtmlSanitizer sanitizer = new();
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.UnionWith(
        [
            "p", "div", "span", "section", "article", "header", "footer", "aside", "nav", "main",
            "h1", "h2", "h3", "h4", "h5", "h6",
            "a", "img", "picture", "source",
            "ul", "ol", "li", "dl", "dt", "dd",
            "table", "thead", "tbody", "tfoot", "tr", "td", "th", "caption", "colgroup", "col",
            "blockquote", "pre", "code", "br", "hr",
            "strong", "em", "b", "i", "u", "s", "sub", "sup", "small", "mark", "ins", "del",
            "figure", "figcaption", "q", "cite", "abbr", "ruby", "rt", "rp", "bdi", "bdo", "wbr",
            "kbd", "samp", "var", "time", "summary", "details"
        ]);
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.UnionWith(
        [
            "class", "id", "title", "lang", "dir", "align", "valign", "width", "height",
            "colspan", "rowspan", "scope", "href", "src", "alt", "data-lumina-resource"
        ]);
        // The style attribute is what keeps the original look of a book (fonts, colors, alignment), but its CSS can also reference
        // resources from external servers, which those servers could observe; whether it survives sanitization is a per-user choice.
        if (shouldPreserveStyles)
            sanitizer.AllowedAttributes.Add("style");
        // Only the safe web schemes are allowed, so that a reference to javascript or a custom scheme is stripped; the data-lumina-resource attribute carries an opaque key instead of a URL, so it needs no scheme.
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto", "tel"]);
        sanitizer.UriAttributes.Clear();
        sanitizer.UriAttributes.UnionWith(["href", "src"]);
        return sanitizer;
    }
}
