#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Common.Metadata;

/// <summary>
/// Extension methods for converting <see cref="TagDto"/>.
/// </summary>
public static class TagDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="Tag"/>.
    /// </summary>
    /// <param name="dto">The DTO to be converted.</param>
    /// <returns>The converted domain entity.</returns>
    public static ErrorOr<Tag> ToDomainEntity(this TagDto dto)
    {
        return Tag.Create(
            dto.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="dtos"/> to a collection of <see cref="Tag"/>.
    /// </summary>
    /// <param name="dtos">The DTOs to be converted.</param>
    /// <returns>The converted domain entities.</returns>
    public static IEnumerable<ErrorOr<Tag>> ToDomainEntities(this IEnumerable<TagDto> dtos)
    {
        return dtos.Select(domainEntity => domainEntity.ToDomainEntity());
    }
}
