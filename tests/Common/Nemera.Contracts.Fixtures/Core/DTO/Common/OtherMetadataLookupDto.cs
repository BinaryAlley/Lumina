#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.DTO.Common;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Contracts.Fixtures.Core.DTO.Common;

/// <summary>
/// Test double deriving from <see cref="MetadataLookupDto"/> for a runtime type other than <see cref="BookMetadataLookupDto"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record OtherMetadataLookupDto : MetadataLookupDto;
