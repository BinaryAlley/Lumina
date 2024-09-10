namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;

/// <summary>
/// Interface defining an factory for creating platform contexts.
/// </summary>
public interface IPlatformContextFactory
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates and returns the appropriate platform context.
    /// </summary>
    /// <typeparam name="TIPlatformContext">The type of platform context to create.</typeparam>
    /// <returns>The created platform context.</returns>
    TIPlatformContext CreateStrategy<TIPlatformContext>() where TIPlatformContext : IPlatformContext;
    #endregion
}