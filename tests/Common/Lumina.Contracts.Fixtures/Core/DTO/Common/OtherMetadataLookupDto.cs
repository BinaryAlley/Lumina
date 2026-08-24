#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.Common;

/// <summary>
/// Test double deriving from <see cref="MetadataLookupDto"/> for a runtime type other than <see cref="BookMetadataLookupDto"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record OtherMetadataLookupDto : MetadataLookupDto;
