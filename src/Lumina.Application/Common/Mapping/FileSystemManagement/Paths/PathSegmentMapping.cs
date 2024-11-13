#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Extension methods for converting <see cref="PathSegment"/>.
/// </summary>
public static class PathSegmentMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="PathSegmentResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PathSegmentResponse ToResponse(this PathSegment domainEntity)
    {
        return new PathSegmentResponse(
            domainEntity.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="PathSegmentResponse"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<PathSegmentResponse> ToResponses(this IEnumerable<PathSegment> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToResponse());
    }
}
