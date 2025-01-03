#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;

/// <summary>
/// Value Object for the name of a person.
/// </summary>
[DebuggerDisplay("{DisplayName}")]
public sealed class MediaContributorName : ValueObject
{
    /// <summary>
    /// Gets the name by which the contributor is popularly known.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the legal name of the contributor.
    /// </summary>
    public Optional<string> LegalName { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContributorName"/> class.
    /// </summary>
    /// <param name="displayName">The name by which the contributor is popularly known.</param>
    /// <param name="legalName">The legal name of the contributor.</param>
    private MediaContributorName(string displayName, Optional<string> legalName)
    {
        DisplayName = displayName;
        LegalName = legalName;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributorName"/> class.
    /// </summary>
    /// <param name="displayName">The name by which the contributor is popularly known.</param>
    /// <param name="legalName">The legal name of the contributor.</param>
    /// <returns>The created <see cref="MediaContributorName"/> instance.</returns>
    public static ErrorOr<MediaContributorName> Create(string displayName, Optional<string> legalName)
    {
        // TODO: enforce invariants
        return new MediaContributorName(displayName, legalName);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return DisplayName;
        yield return LegalName;
    }
}
