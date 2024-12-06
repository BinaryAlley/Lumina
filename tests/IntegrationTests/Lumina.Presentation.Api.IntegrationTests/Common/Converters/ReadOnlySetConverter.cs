#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Converters;

/// <summary>
/// Custom JSON converter for readonly collections.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadOnlySetConverter<T> : JsonConverter<IReadOnlySet<T>>
{
    /// <summary>
    /// Reads and converts JSON to a <see cref="IReadOnlySet{T}"/> object.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type of the object to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted readonly collection.</returns>
    public override IReadOnlySet<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        HashSet<T>? list = JsonSerializer.Deserialize<HashSet<T>>(ref reader, options);
        return list ?? [];
    }

    /// <summary>
    /// Writes a readonly collection object to JSON. This method is not implemented.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The <see cref="IReadOnlySet{T}"/> object to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, IReadOnlySet<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToList(), options);
    }
}
