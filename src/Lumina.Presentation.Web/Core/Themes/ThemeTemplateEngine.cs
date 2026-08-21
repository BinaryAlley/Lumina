#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// A small, non-executable Mustache-style renderer. Theme templates can place values, iterate collections and test truthiness, but cannot execute C#.
/// </summary>
public sealed class ThemeTemplateEngine
{
    // bound template nesting and rendered output, so a single theme cannot exhaust the server memory
    private const int MAX_NESTING_DEPTH = 32;
    private const int MAX_RENDERED_CHARACTERS = 4 * 1024 * 1024;

    private static readonly Regex s_namePartPattern = new(
        "^[A-Za-z_][A-Za-z0-9_]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Renders a theme template against the provided model, splitting the reserved top-level <c>scripts</c> section from the page content.
    /// </summary>
    /// <param name="template">The template source to render.</param>
    /// <param name="model">The model the template expressions resolve against.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the rendered page, or an error.</returns>
    public Result<ThemePageRenderResultDto> RenderPage(string template, object model)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(model);

        int cursor = 0;
        Result<IReadOnlyList<ThemeTemplateNodeDto>> parseResult = ParseNodes(template, ref cursor, expectedClosingName: null, depth: 0);
        if (parseResult.IsFailure)
            return parseResult.Errors;

        // the reserved top-level 'scripts' section holds the script of the page and is rendered separately, mirroring the
        // 'Scripts' section of a Razor view, so that the layout can host it in the scripts container of the navigator;
        // every other top-level node is the content of the page section
        List<ThemeTemplateNodeDto> contentNodes = [];
        ThemeSectionNodeDto? scriptsSection = null;
        foreach (ThemeTemplateNodeDto node in parseResult.Value)
        {
            if (node is ThemeSectionNodeDto { Inverted: false } section
                && scriptsSection is null
                && string.Equals(section.Expression, "scripts", StringComparison.OrdinalIgnoreCase))
            {
                scriptsSection = section;
                continue;
            }

            contentNodes.Add(node);
        }

        StringBuilder contentOutput = new(Math.Min(template.Length * 2, 256 * 1024));
        Result<Success> contentResult = RenderNodes(contentNodes, new ThemeRenderScopeDto(model, Parent: null), contentOutput);
        if (contentResult.IsFailure)
            return contentResult.Errors;

        StringBuilder scriptOutput = new(64 * 1024);
        if (scriptsSection is not null)
        {
            Result<Success> scriptResult = RenderNodes(scriptsSection.Children, new ThemeRenderScopeDto(model, Parent: null), scriptOutput);
            if (scriptResult.IsFailure)
                return scriptResult.Errors;
        }

        return new ThemePageRenderResultDto(contentOutput.ToString(), scriptOutput.ToString());
    }

    /// <summary>
    /// Parses a template fragment into template nodes until the closing section or end of the template.
    /// </summary>
    /// <param name="template">The template source to parse.</param>
    /// <param name="cursor">The current position in the template, advanced as nodes are consumed.</param>
    /// <param name="expectedClosingName">The name of the section that closes the fragment, or <see langword="null"/> at the top level.</param>
    /// <param name="depth">The current section nesting depth.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the parsed nodes, or an error.</returns>
    private static Result<IReadOnlyList<ThemeTemplateNodeDto>> ParseNodes(string template, ref int cursor, string? expectedClosingName, int depth)
    {
        if (depth > MAX_NESTING_DEPTH)
            return TemplateInvalid($"Template sections may not be nested more than {MAX_NESTING_DEPTH} levels.");

        List<ThemeTemplateNodeDto> nodes = [];
        while (cursor < template.Length)
        {
            int opening = template.IndexOf("{{", cursor, StringComparison.Ordinal);
            if (opening < 0)
            {
                if (cursor < template.Length)
                    nodes.Add(new ThemeTextNodeDto(template[cursor..]));

                cursor = template.Length;
                break;
            }

            if (opening > cursor)
                nodes.Add(new ThemeTextNodeDto(template[cursor..opening]));

            if (template.AsSpan(opening).StartsWith("{{{".AsSpan(), StringComparison.Ordinal))
            {
                int closing = template.IndexOf("}}}", opening + 3, StringComparison.Ordinal);
                if (closing < 0)
                    return TemplateInvalid("An unescaped variable tag is not closed.");

                string expression = template[(opening + 3)..closing].Trim();
                Result<Success> expressionResult = ValidateExpression(expression);
                if (expressionResult.IsFailure)
                    return expressionResult.Errors;

                nodes.Add(new ThemeVariableNodeDto(expression, ShouldBeEscaped: false));
                cursor = closing + 3;
                continue;
            }

            int tagClosing = template.IndexOf("}}", opening + 2, StringComparison.Ordinal);
            if (tagClosing < 0)
                return TemplateInvalid("A template tag is not closed.");

            string tag = template[(opening + 2)..tagClosing].Trim();
            cursor = tagClosing + 2;
            if (tag.Length == 0)
                return TemplateInvalid("Empty template tags are not allowed.");

            switch (tag[0])
            {
                case '!':
                    continue;
                case '#':
                case '^':
                    {
                        string expression = tag[1..].Trim();
                        Result<Success> sectionExpressionResult = ValidateExpression(expression);
                        if (sectionExpressionResult.IsFailure)
                            return sectionExpressionResult.Errors;

                        Result<IReadOnlyList<ThemeTemplateNodeDto>> childrenResult = ParseNodes(template, ref cursor, expression, depth + 1);
                        if (childrenResult.IsFailure)
                            return childrenResult.Errors;

                        nodes.Add(new ThemeSectionNodeDto(expression, Inverted: tag[0] == '^', Children: childrenResult.Value));
                        break;
                    }
                case '/':
                    {
                        string closingName = tag[1..].Trim();
                        Result<Success> closingNameResult = ValidateExpression(closingName);
                        if (closingNameResult.IsFailure)
                            return closingNameResult.Errors;

                        if (expectedClosingName is null)
                            return TemplateInvalid($"Closing section '{closingName}' has no matching opening section.");

                        if (!string.Equals(closingName, expectedClosingName, StringComparison.Ordinal))
                            return TemplateInvalid($"Section '{expectedClosingName}' is closed by '{closingName}'.");

                        return nodes;
                    }
                case '&':
                    {
                        string expression = tag[1..].Trim();
                        Result<Success> unescapedExpressionResult = ValidateExpression(expression);
                        if (unescapedExpressionResult.IsFailure)
                            return unescapedExpressionResult.Errors;

                        nodes.Add(new ThemeVariableNodeDto(expression, ShouldBeEscaped: false));
                        break;
                    }
                default:
                    {
                        Result<Success> variableResult = ValidateExpression(tag);
                        if (variableResult.IsFailure)
                            return variableResult.Errors;

                        nodes.Add(new ThemeVariableNodeDto(tag, ShouldBeEscaped: true));
                        break;
                    }
            }
        }

        if (expectedClosingName is not null)
            return TemplateInvalid($"Section '{expectedClosingName}' is not closed.");

        return nodes;
    }

    /// <summary>
    /// Renders a list of template nodes into the output.
    /// </summary>
    /// <param name="nodes">The template nodes to render.</param>
    /// <param name="scope">The model scope the expressions resolve against.</param>
    /// <param name="output">The output the rendered text is appended to.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private static Result<Success> RenderNodes(IReadOnlyList<ThemeTemplateNodeDto> nodes, ThemeRenderScopeDto scope, StringBuilder output)
    {
        foreach (ThemeTemplateNodeDto node in nodes)
        {
            switch (node)
            {
                case ThemeTextNodeDto text:
                    {
                        Result<Success> textResult = AppendChecked(output, text.Value);
                        if (textResult.IsFailure)
                            return textResult.Errors;

                        break;
                    }
                case ThemeVariableNodeDto variable:
                    {
                        object? resolved = Resolve(variable.Expression, scope);
                        string value = ConvertToString(resolved);
                        Result<Success> variableResult = AppendChecked(output, variable.ShouldBeEscaped ? HtmlEncoder.Default.Encode(value) : value);
                        if (variableResult.IsFailure)
                            return variableResult.Errors;

                        break;
                    }
                case ThemeSectionNodeDto section:
                    {
                        Result<Success> sectionResult = RenderSection(section, scope, output);
                        if (sectionResult.IsFailure)
                            return sectionResult.Errors;

                        break;
                    }
            }
        }

        return Result.Success;
    }

    /// <summary>
    /// Renders a section by evaluating its expression and iterating or conditionally rendering its children.
    /// </summary>
    /// <param name="section">The section node to render.</param>
    /// <param name="scope">The model scope the section expression resolves against.</param>
    /// <param name="output">The output the rendered text is appended to.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private static Result<Success> RenderSection(ThemeSectionNodeDto section, ThemeRenderScopeDto scope, StringBuilder output)
    {
        object? value = Resolve(section.Expression, scope);
        bool truthy = IsTruthy(value);
        if (section.Inverted)
        {
            if (!truthy)
                return RenderNodes(section.Children, scope, output);

            return Result.Success;
        }

        if (!truthy)
            return Result.Success;

        if (value is bool)
            return RenderNodes(section.Children, scope, output);

        if (value is System.Collections.IEnumerable enumerable and not string)
        {
            foreach (object? item in enumerable)
            {
                Result<Success> itemResult = RenderNodes(section.Children, new ThemeRenderScopeDto(item, scope), output);
                if (itemResult.IsFailure)
                    return itemResult.Errors;
            }

            return Result.Success;
        }

        return RenderNodes(section.Children, new ThemeRenderScopeDto(value, scope), output);
    }

    /// <summary>
    /// Resolves a dotted expression against the scope chain, honoring explicit parent traversal.
    /// </summary>
    /// <param name="expression">The dotted property expression to resolve.</param>
    /// <param name="startingScope">The scope the expression is resolved from.</param>
    /// <returns>The resolved value, or <see langword="null"/> when no scope in the chain exposes it.</returns>
    private static object? Resolve(string expression, ThemeRenderScopeDto startingScope)
    {
        int explicitParentCount = 0;
        while (expression.StartsWith("../", StringComparison.Ordinal))
        {
            explicitParentCount++;
            expression = expression[3..];
        }

        ThemeRenderScopeDto scope = startingScope;
        for (int index = 0; index < explicitParentCount; index++)
            scope = scope.Parent ?? scope;

        if (expression == ".")
            return scope.Value;

        if (explicitParentCount > 0)
            return TryResolvePath(scope.Value, expression, out object? explicitValue) ? explicitValue : null;

        for (ThemeRenderScopeDto? candidate = scope; candidate is not null; candidate = candidate.Parent)
            if (TryResolvePath(candidate.Value, expression, out object? value))
                return value;

        return null;
    }

    /// <summary>
    /// Resolves a dotted property path against a value, member by member.
    /// </summary>
    /// <param name="value">The value the path is resolved against.</param>
    /// <param name="expression">The dotted property path to resolve.</param>
    /// <param name="resolved">The resolved value, or <see langword="null"/> when any part of the path is missing.</param>
    /// <returns><see langword="true"/> when the full path resolved; <see langword="false"/> otherwise.</returns>
    private static bool TryResolvePath(object? value, string expression, out object? resolved)
    {
        resolved = value;
        foreach (string part in expression.Split('.'))
        {
            if (!TryReadMember(resolved, part, out resolved))
            {
                resolved = null;
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Reads a single member from a value, supporting dictionaries, collections and public properties.
    /// </summary>
    /// <param name="target">The value the member is read from.</param>
    /// <param name="name">The member name to read, matched case-insensitively.</param>
    /// <param name="value">The member value, or <see langword="null"/> when the member is not found.</param>
    /// <returns><see langword="true"/> when the member was found; <see langword="false"/> otherwise.</returns>
    private static bool TryReadMember(object? target, string name, out object? value)
    {
        value = null;
        if (target is null)
            return false;

        if (target is IReadOnlyDictionary<string, object?> readOnlyDictionary)
        {
            foreach (KeyValuePair<string, object?> pair in readOnlyDictionary)
            {
                if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = pair.Value;
                    return true;
                }
            }

            return false;
        }

        if (target is IDictionary<string, object?> dictionary)
        {
            foreach (KeyValuePair<string, object?> pair in dictionary)
            {
                if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = pair.Value;
                    return true;
                }
            }

            return false;
        }

        if (string.Equals(name, "Count", StringComparison.OrdinalIgnoreCase) && target is System.Collections.ICollection collection)
        {
            value = collection.Count;
            return true;
        }

        PropertyInfo? property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property is null || property.GetIndexParameters().Length != 0)
            return false;

        value = property.GetValue(target);
        return true;
    }

    /// <summary>
    /// Determines the truthiness of a value: <see langword="null"/> and <see langword="false"/> are falsy, empty strings and empty collections are falsy.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <returns><see langword="true"/> when the value is truthy; <see langword="false"/> otherwise.</returns>
    private static bool IsTruthy(object? value)
    {
        if (value is null or false)
            return false;

        if (value is string text)
            return text.Length > 0;

        if (value is System.Collections.IEnumerable enumerable)
        {
            System.Collections.IEnumerator enumerator = enumerable.GetEnumerator();
            try
            {
                return enumerator.MoveNext();
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        return true;
    }

    /// <summary>
    /// Converts a resolved value to its rendered string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The rendered string, or an empty string for <see langword="null"/> values.</returns>
    private static string ConvertToString(object? value)
    {
        return value switch
        {
            null => string.Empty,
            bool boolean => boolean ? "true" : "false",
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Validates a template expression: non-empty, within the length limit and composed of dotted name parts.
    /// </summary>
    /// <param name="expression">The expression to validate.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private static Result<Success> ValidateExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression) || expression.Length > 120)
            return TemplateInvalid("A template expression is empty or too long.");

        string remaining = expression;
        while (remaining.StartsWith("../", StringComparison.Ordinal))
            remaining = remaining[3..];

        if (remaining == ".")
            return Result.Success;

        if (remaining.Length == 0 || remaining.Split('.').Any(part => !s_namePartPattern.IsMatch(part)))
            return TemplateInvalid($"Expression '{expression}' is invalid. Use property names separated by dots.");

        return Result.Success;
    }

    /// <summary>
    /// Appends a value to the output, enforcing the maximum rendered character limit.
    /// </summary>
    /// <param name="output">The output the value is appended to.</param>
    /// <param name="value">The value to append.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private static Result<Success> AppendChecked(StringBuilder output, string value)
    {
        if (output.Length + value.Length > MAX_RENDERED_CHARACTERS)
            return TemplateInvalid("Rendered output exceeds the 4 MB safety limit.");

        output.Append(value);
        return Result.Success;
    }

    /// <summary>
    /// Creates a validation error for an invalid theme template.
    /// </summary>
    /// <param name="description">The human-readable description of the template error.</param>
    /// <returns>The validation error describing the invalid theme template.</returns>
    private static Error TemplateInvalid(string description)
    {
        return Error.Validation(code: "Theme.Template.Invalid", description: description);
    }
}
