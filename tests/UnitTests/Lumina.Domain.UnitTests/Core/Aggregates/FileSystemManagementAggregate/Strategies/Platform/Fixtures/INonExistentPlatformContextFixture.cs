#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Strategies.Platform.Fixtures;

/// <summary>
/// Fixture interface for non existing platform context.
/// </summary>
public interface INonExistentPlatformContextFixture : IPlatformContext
{
}
