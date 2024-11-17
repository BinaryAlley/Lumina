#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Provides functionality to manage unique script identifiers across views.
/// </summary>
public static class ScriptIdentifierHelper
{
    /// <summary>
    /// Stores currently used script identifiers to prevent duplicates.
    /// </summary>
    private static readonly HashSet<string> s_usedIdentifiers = new();

    /// <summary>
    /// Generates a unique script identifier using a GUID.
    /// </summary>
    /// <returns>A unique identifier in format "script_{guid}"</returns>
    /// <exception cref="InvalidOperationException">Thrown if a duplicate identifier is detected.</exception>
    public static string GenerateScriptId()
    {
        string scriptId = $"script_{Guid.NewGuid():N}";
        if (!s_usedIdentifiers.Add(scriptId))
            throw new InvalidOperationException($"Script identifier {scriptId} is already in use!");
        return scriptId;
    }

    /// <summary>
    /// Removes a script identifier from tracking when it's no longer needed.
    /// </summary>
    /// <param name="scriptId">The script identifier to remove.</param>
    public static void ClearIdentifier(string scriptId)
    {
        s_usedIdentifiers.Remove(scriptId);
    }
}
