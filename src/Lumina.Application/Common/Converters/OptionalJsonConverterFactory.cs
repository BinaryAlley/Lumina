#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Application.Common.Converters;

/// <summary>
/// A factory for creating <see cref="OptionalJsonConverter{T}"/> instances.
/// </summary>
public class OptionalJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether the converter can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check for convertibility.</param>
    /// <returns><see langword="true"/> if the type is <see cref="Optional{TValue}"/>, <see langword="false"/> otherwise.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type for which to create a converter.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>A converter for <see cref="Optional{TValue}"/>.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type valueType = typeToConvert.GetGenericArguments()[0];
        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(OptionalJsonConverter<>).MakeGenericType(valueType))!;
        return converter;
    }
}