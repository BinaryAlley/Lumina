#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.Management;

/// <summary>
/// Represents a path part definition response.
/// </summary>
/// <param name="Kind">The kind of the path part.</param>
/// <param name="ValueType">The value type captured by the path part.</param>
/// <param name="DefaultRepresentation">The default representation of the path part, pre-filled when it is added to a template.</param>
/// <param name="IsOptionalByDefault">Whether the path part is optional by default.</param>
[DebuggerDisplay("Kind: {Kind}; ValueType: {ValueType}")]
public record LibraryPathPartDefinitionResponse(
    string Kind,
    string ValueType,
    string DefaultRepresentation,
    bool IsOptionalByDefault
);
