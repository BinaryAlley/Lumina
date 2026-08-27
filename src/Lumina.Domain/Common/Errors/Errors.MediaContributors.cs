#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Media contributor error types.
/// </summary>
public static partial class Errors
{
    public static class MediaContributors
    {
        public static Error MediaContributorNameCannotBeEmpty => Error.Validation(description: nameof(MediaContributorNameCannotBeEmpty));
        public static Error MediaContributorRoleNameCannotBeEmpty => Error.Validation(description: nameof(MediaContributorRoleNameCannotBeEmpty));
    }
}
