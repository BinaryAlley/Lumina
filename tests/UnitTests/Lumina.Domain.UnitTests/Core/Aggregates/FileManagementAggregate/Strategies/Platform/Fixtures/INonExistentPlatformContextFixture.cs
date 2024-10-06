#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Strategies.Platform.Fixtures;

/// <summary>
/// Fixture interface for non existing platform context.
/// </summary>
public interface INonExistentPlatformContextFixture : IPlatformContext
{
}
