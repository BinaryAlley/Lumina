#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Application.Common.Converters;

/// <summary>
/// Converts an <see cref="Optional{TValue}"/> to and from JSON.
/// </summary>
/// <typeparam name="TValue">The type of the value represented by the <see cref="Optional{TValue}"/> structure.</typeparam>
public class OptionalJsonConverter<TValue> : JsonConverter<Optional<TValue>>
{
    /// <summary>
    /// Reads and converts the JSON to an instance of <see cref="Optional{TValue}"/>.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The converted <see cref="Optional{TValue}"/> value.</returns>
    public override Optional<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return Optional<TValue>.None();
        TValue? value = JsonSerializer.Deserialize<TValue>(ref reader, options);
        return Optional<TValue>.Some(value!);
    }

    /// <summary>
    /// Writes an <see cref="Optional{TValue}"/> value as JSON.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, Optional<TValue> value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            JsonSerializer.Serialize(writer, value.Value, options);
        else
            writer.WriteNullValue();
    }
}