#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
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
    /// Converts <paramref name="domainModel"/> to <see cref="PathSegmentResponse"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PathSegmentResponse ToResponse(this PathSegment domainModel)
    {
        return new PathSegmentResponse(
            domainModel.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="domainModels"/> to a collection of <see cref="PathSegmentResponse"/>.
    /// </summary>
    /// <param name="domainModels">The domain models to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IEnumerable<PathSegmentResponse> ToResponses(this IEnumerable<PathSegment> domainModels)
    {
        return domainModels.Select(domainModel => domainModel.ToResponse());
    }
}
