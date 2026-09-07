#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;

/// <summary>
/// Enumeration for the type of a media contributor, distinguishing whether the contributor is a single person,
/// a group of people, or an organization.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MediaContributorType
{
    /// <summary>
    /// The contributor is a single person.
    /// </summary>
    Person,

    /// <summary>
    /// The contributor is a group of people, like a band or an orchestra.
    /// </summary>
    Group,

    /// <summary>
    /// The contributor is an organization, like a record label or a studio.
    /// </summary>
    Organization,

    /// <summary>
    /// The type of the contributor does not fall into any of the known categories.
    /// </summary>
    Other
}
