#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Contracts.DTO.Common.Filtering;

/// <summary>
/// Data transfer object that represents the parameters required to filter by library data.
/// </summary>
public record LibraryFilterDto : BaseFilterDto
{
    /// <summary>
    /// The Id of the media library used to filter data.
    /// </summary>
    public required Guid LibraryId { get; init; }
}
