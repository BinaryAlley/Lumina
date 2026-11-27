#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Concurrent;
using System.Reflection;
#endregion

namespace Lumina.Application.Common.Telemetry;

/// <summary>
/// Detects the outcome of an application handler result in a generic way, without coupling the telemetry decorators to any concrete result type.
/// </summary>
internal static class HandlerOutcomeDetector
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> s_isSuccessPropertyCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> s_firstErrorPropertyCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> s_errorDescriptionPropertyCache = new();

    /// <summary>
    /// Determines whether the specified result represents a successful outcome.
    /// </summary>
    /// <param name="result">The result returned by an application handler, or <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> when the result reports success, when it is <see langword="null"/>, or when it exposes no success flag.
    /// </returns>
    /// <remarks>
    /// Handlers that return plain DTOs (no <c>IsSuccess</c> property) cannot express a failure and are reported as successful;
    /// only handlers returning a result type such as <see cref="Domain.Common.Primitives.Result{TValue}"/> report failures.
    /// </remarks>
    public static bool IsSuccessful(object? result)
    {
        if (result is null)
            return true;
        PropertyInfo? isSuccessProperty = GetIsSuccessProperty(result.GetType());
        return isSuccessProperty is null || (bool)isSuccessProperty.GetValue(result)!;
    }

    /// <summary>
    /// Gets a human-readable description of the first error of a failed result, when the result exposes one.
    /// </summary>
    /// <param name="result">The failed result returned by an application handler, or <see langword="null"/>.</param>
    /// <returns>The description of the first error, or <see langword="null"/> when the result exposes no errors.</returns>
    public static string? GetErrorDescription(object? result)
    {
        if (result is null)
            return null;
        PropertyInfo? firstErrorProperty = s_firstErrorPropertyCache.GetOrAdd(result.GetType(), type => type.GetProperty("FirstError", BindingFlags.Public | BindingFlags.Instance));
        object? firstError = firstErrorProperty?.GetValue(result);
        if (firstError is null)
            return null;
        PropertyInfo? descriptionProperty = s_errorDescriptionPropertyCache.GetOrAdd(firstError.GetType(), type => type.GetProperty("Description", BindingFlags.Public | BindingFlags.Instance));
        return descriptionProperty?.GetValue(firstError)?.ToString();
    }

    /// <summary>
    /// Gets the public instance <c>IsSuccess</c> property of the specified type, if it has one.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>The <c>IsSuccess</c> property, or <see langword="null"/> when the type does not expose one.</returns>
    private static PropertyInfo? GetIsSuccessProperty(Type type)
    {
        return s_isSuccessPropertyCache.GetOrAdd(type, candidateType => candidateType.GetProperty("IsSuccess", BindingFlags.Public | BindingFlags.Instance));
    }
}
