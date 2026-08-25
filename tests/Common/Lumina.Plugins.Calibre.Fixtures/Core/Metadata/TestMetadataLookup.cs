#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Calibre.Fixtures.Core.Metadata;

/// <summary>
/// Concrete <see cref="MetadataLookupDto"/> used to represent a lookup that is not a book lookup.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed record TestMetadataLookup(Guid LibraryId, string Path) : MetadataLookupDto;
