#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
#endregion

namespace Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;

/// <summary>
/// Interface for the library ownership authorization policy.
/// </summary>
public interface ILibraryOwnershipPolicy : IAuthorizationPolicy
{
}
