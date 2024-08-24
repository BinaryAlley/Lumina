#region ========================================================================= USING =====================================================================================
using System.Text.Json;
using System.Text.Json.Serialization;
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Application.Common.Converters;

/// <summary>
/// Converts an <see cref="Optional{T}"/> to and from JSON.
/// </summary>
/// <typeparam name="T">The type of the value represented by the <see cref="Optional{T}"/> structure.</typeparam>
public class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Reads and converts the JSON to an instance of <see cref="Optional{T}"/>.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The converted <see cref="Optional{T}"/> value.</returns>
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return Optional<T>.None();
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return Optional<T>.Some(value!);
    }

    /// <summary>
    /// Writes an <see cref="Optional{T}"/> value as JSON.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            JsonSerializer.Serialize(writer, value.Value, options);
        else
            writer.WriteNullValue();
    }
    #endregion
}