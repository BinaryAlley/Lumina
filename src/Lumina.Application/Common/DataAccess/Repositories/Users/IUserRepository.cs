#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Users;

/// <summary>
/// Interface for the repository for users.
/// </summary>
public interface IUserRepository : IRepository<UserEntity>,
                                   IInsertRepositoryAction<UserEntity>
{
}
