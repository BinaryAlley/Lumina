#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="ResumePointDto"/>. This mapping is the resume point response builder for repository-driven
/// single-record reads.
/// </summary>
public static class ResumePointDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="ResumePointResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ResumePointResponse ToResponse(this ResumePointDto data)
    {
        return new ResumePointResponse(
            data.Id,
            data.LibraryId,
            data.MediaItemId,
            data.MediaKind,
            data.LastPosition.ToResponse(),
            data.LastProgressFraction,
            DateTime.SpecifyKind(data.LastConsumedOnUtc, DateTimeKind.Utc)
        );
    }
}
