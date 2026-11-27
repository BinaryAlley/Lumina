#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Common;
using Lumina.Presentation.Api.Fixtures.Core.Endpoints.FileSystemManagement;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Xunit;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Marks a test to be skipped when the file system does not support the <see cref="FileAttributes.Hidden"/> attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[ExcludeFromCodeCoverage]
public sealed class SkipWhenHiddenAttributeNotSupportedFactAttribute : FactAttribute
{
    /// <summary>
    /// Gets the reason to skip the test. If set, the test will be skipped instead of executed.
    /// </summary>
    public override string Skip => FileSystemStructureFixture.HiddenAttributeIsSupported
        ? string.Empty
        : "The file system does not support the hidden file attribute, so the hidden elements scenario cannot be set up.";
}
