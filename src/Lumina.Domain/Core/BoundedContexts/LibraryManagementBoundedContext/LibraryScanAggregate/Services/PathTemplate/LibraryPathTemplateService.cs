#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;

/// <summary>
/// Validates media library path templates and derives metadata from paths using them.
/// </summary>
public sealed class LibraryPathTemplateService : ILibraryPathTemplateService
{
    /// <summary>
    /// Matches the value placeholder of a typed part, capturing the optional fixed digit width.
    /// </summary>
    private static readonly Regex s_placeholderRegex = new(@"\{0(?::(?<width>0+))?\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly IReadOnlyList<ILibraryPathPartCatalog> _catalogs;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPathTemplateService"/> class.
    /// </summary>
    /// <param name="catalogs">The path part catalogs of all the supported media library types.</param>
    public LibraryPathTemplateService(IEnumerable<ILibraryPathPartCatalog> catalogs)
    {
        _catalogs = [.. catalogs];
    }

    /// <summary>
    /// Gets the default path template of the provided <paramref name="libraryType"/>, or an empty template when the type has no path part catalog yet.
    /// </summary>
    /// <param name="libraryType">The media library type whose default path template is retrieved.</param>
    /// <returns>The default path template.</returns>
    public LibraryPathTemplate GetDefaultTemplate(LibraryType libraryType)
    {
        return ResolveCatalog(libraryType)?.GetDefaultTemplate() ?? LibraryPathTemplate.Empty();
    }

    /// <summary>
    /// Validates the provided <paramref name="template"/> against the catalog of the provided <paramref name="libraryType"/>.
    /// </summary>
    /// <param name="libraryType">The media library type the template belongs to.</param>
    /// <param name="template">The path template to validate.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Success> Validate(LibraryType libraryType, LibraryPathTemplate template)
    {
        if (template.IsEmpty)
            return Result.Success;

        ILibraryPathPartCatalog? catalog = ResolveCatalog(libraryType);
        if (catalog is null)
            return DomainErrors.Library.PathTemplateIsNotSupportedForLibraryType;

        HashSet<LibraryPathPartKind> supportedKinds = [.. catalog.GetPartDefinitions().Select(definition => definition.Kind)];
        foreach (LibraryPathPart part in template.Parts)
            if (!supportedKinds.Contains(part.Kind))
                return DomainErrors.Library.PathTemplatePartKindNotSupportedForLibraryType;
        return Result.Success;
    }

    /// <summary>
    /// Derives the metadata of a media library item from its relative path, using the provided <paramref name="template"/>.
    /// </summary>
    /// <param name="libraryType">The media library type the template belongs to.</param>
    /// <param name="template">The path template used to parse the path.</param>
    /// <param name="relativePath">The path of the media library item, relative to the root of its content location.</param>
    /// <param name="pathSeparator">The character used to separate path segments on the current platform.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the derived metadata, or an error. A successful result whose value is <see langword="null"/>
    /// means the path does not match the template.
    /// </returns>
    public Result<ParsedLibraryPath?> Parse(LibraryType libraryType, LibraryPathTemplate template, string relativePath, char pathSeparator)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || template.IsEmpty)
            return Result<ParsedLibraryPath?>.Success(null);

        Result<Success> validationResult = Validate(libraryType, template);
        if (validationResult.IsFailure)
            return validationResult.Errors;

        ILibraryPathPartCatalog catalog = ResolveCatalog(libraryType)!;
        Dictionary<LibraryPathPartKind, LibraryPathPartDefinition> definitions = catalog.GetPartDefinitions()
            .ToDictionary(definition => definition.Kind);
        Regex pattern = BuildPattern(template, definitions, pathSeparator);
        Match match = pattern.Match(relativePath);
        if (!match.Success)
            return Result<ParsedLibraryPath?>.Success(null);

        Dictionary<LibraryPathPartKind, string> values = [];
        foreach (LibraryPathPart part in template.Parts)
        {
            if (part.Kind == LibraryPathPartKind.Literal || part.Kind == LibraryPathPartKind.Separator)
                continue;
            // a kind can legitimately appear more than once, like the title and the author appearing both in the directory and in the file name,
            // so the first non-empty capture is kept.
            if (values.ContainsKey(part.Kind))
                continue;
            Group group = match.Groups[part.Kind.ToString()];
            if (!group.Success)
                continue;
            string? value = group.Captures.Cast<Capture>().Select(capture => capture.Value).FirstOrDefault(captured => !string.IsNullOrWhiteSpace(captured));
            if (!string.IsNullOrWhiteSpace(value))
                values[part.Kind] = value;
        }
        return Result<ParsedLibraryPath?>.Success(ParsedLibraryPath.Create(values));
    }

    /// <summary>
    /// Resolves the catalog of the provided <paramref name="libraryType"/>.
    /// </summary>
    /// <param name="libraryType">The media library type whose catalog is resolved.</param>
    /// <returns>The resolved catalog, or <see langword="null"/> when the type has no catalog.</returns>
    private ILibraryPathPartCatalog? ResolveCatalog(LibraryType libraryType)
    {
        return _catalogs.FirstOrDefault(catalog => catalog.SupportedLibraryType == libraryType);
    }

    /// <summary>
    /// Builds the regular expression that matches the provided <paramref name="template"/>.
    /// </summary>
    /// <param name="template">The path template to compile.</param>
    /// <param name="definitions">The available part definitions of the template's library type.</param>
    /// <param name="pathSeparator">The character used to separate path segments on the current platform.</param>
    /// <returns>The compiled regular expression.</returns>
    private static Regex BuildPattern(LibraryPathTemplate template, IReadOnlyDictionary<LibraryPathPartKind, LibraryPathPartDefinition> definitions, char pathSeparator)
    {
        StringBuilder patternBuilder = new("^");
        int index = 0;
        while (index < template.Parts.Count)
        {
            int runStart = index;
            bool isOptionalRun = template.Parts[index].IsOptional;
            do
            {
                index++;
            }
            while (index < template.Parts.Count && template.Parts[index].IsOptional == isOptionalRun);

            StringBuilder runBuilder = new();
            for (int runIndex = runStart; runIndex < index; runIndex++)
                runBuilder.Append(BuildFragment(template.Parts[runIndex], definitions, pathSeparator));

            if (isOptionalRun)
                patternBuilder.Append("(?:").Append(runBuilder).Append(")?");
            else
                patternBuilder.Append(runBuilder);
        }
        patternBuilder.Append('$');
        return new Regex(patternBuilder.ToString(), RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

    /// <summary>
    /// Builds the regular expression fragment of a single path part.
    /// </summary>
    /// <param name="part">The path part to compile.</param>
    /// <param name="definitions">The available part definitions of the template's library type.</param>
    /// <param name="pathSeparator">The character used to separate path segments on the current platform.</param>
    /// <returns>The compiled regular expression fragment.</returns>
    private static string BuildFragment(LibraryPathPart part, IReadOnlyDictionary<LibraryPathPartKind, LibraryPathPartDefinition> definitions, char pathSeparator)
    {
        if (part.Kind == LibraryPathPartKind.Separator)
            return Regex.Escape(pathSeparator.ToString());
        if (part.Kind == LibraryPathPartKind.Literal)
            return Regex.Escape(part.Representation);

        LibraryPathPartDefinition definition = definitions[part.Kind];
        Match placeholder = s_placeholderRegex.Match(part.Representation);
        string prefix = part.Representation[..placeholder.Index];
        string suffix = part.Representation[(placeholder.Index + placeholder.Length)..];
        Group width = placeholder.Groups["width"];
        // a fixed width narrows the capture to an exact number of digits, otherwise the base pattern of the value type is used.
        string valuePattern = width.Success ? $@"\d{{{width.Value.Length}}}" : GetValuePattern(definition.ValueType);
        return Regex.Escape(prefix) + $"(?<{part.Kind}>{valuePattern})" + Regex.Escape(suffix);
    }

    /// <summary>
    /// Gets the base regular expression pattern of the provided <paramref name="valueType"/>.
    /// </summary>
    /// <param name="valueType">The value type whose base pattern is retrieved.</param>
    /// <returns>The base regular expression pattern.</returns>
    private static string GetValuePattern(LibraryPathValueType valueType)
    {
        return valueType switch
        {
            LibraryPathValueType.Year => @"\d{4}",
            LibraryPathValueType.Integer => @"\d+",
            _ => @"[^/\\]+"
        };
    }
}
