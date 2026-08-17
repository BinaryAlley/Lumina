#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Services;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Services;

/// <summary>
/// Contains unit tests for the <see cref="ScriptIdentifierHelper"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScriptIdentifierHelperTests
{
    [Fact]
    public void GenerateScriptId_WhenCalled_ShouldReturnScriptPrefixedIdentifier()
    {
        // Act
        string scriptId = ScriptIdentifierHelper.GenerateScriptId();

        // Assert
        Assert.StartsWith("script_", scriptId);
        Assert.True(Guid.TryParse(scriptId.AsSpan("script_".Length), out _));
    }

    [Fact]
    public void GenerateScriptId_WhenCalledTwice_ShouldReturnDistinctIdentifiers()
    {
        // Act
        string firstScriptId = ScriptIdentifierHelper.GenerateScriptId();
        string secondScriptId = ScriptIdentifierHelper.GenerateScriptId();

        // Assert
        Assert.NotEqual(firstScriptId, secondScriptId);
    }

    [Fact]
    public void ClearIdentifier_WhenCalledWithGeneratedIdentifier_ShouldNotThrow()
    {
        // Arrange
        string scriptId = ScriptIdentifierHelper.GenerateScriptId();

        // Act
        ScriptIdentifierHelper.ClearIdentifier(scriptId);

        // Assert
        Assert.NotNull(scriptId);
    }
}
