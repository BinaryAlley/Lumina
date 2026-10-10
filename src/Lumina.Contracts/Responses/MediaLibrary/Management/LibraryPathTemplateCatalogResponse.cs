#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.Management;

/// <summary>
/// Represents a path template catalog response.
/// </summary>
/// <param name="Parts">The path parts that can be used in the path template of the library type.</param>
/// <param name="DefaultTemplateParts">The ordered parts of the default path template of the library type, describing the ideal structure the application expects.</param>
[DebuggerDisplay("Parts: {Parts.Count}; DefaultTemplateParts: {DefaultTemplateParts.Count}")]
public record LibraryPathTemplateCatalogResponse(
    List<LibraryPathPartDefinitionResponse> Parts,
    List<LibraryPathTemplatePartResponse> DefaultTemplateParts
);
