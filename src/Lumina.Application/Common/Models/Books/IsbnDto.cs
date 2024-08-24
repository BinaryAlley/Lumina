#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Enums;
#endregion

namespace Lumina.Application.Common.Models.Books;

/// <summary>
/// Represents a request to get ISBN information.
/// </summary>
public record IsbnDto(
    string Value,
    IsbnFormat Format
);