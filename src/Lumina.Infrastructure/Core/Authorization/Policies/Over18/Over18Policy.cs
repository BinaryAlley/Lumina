#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Over18;
using Lumina.Application.Common.Infrastructure.Time;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Authorization.Policies.Over18;

/// <summary>
/// Authorization policy that checks if a user if over 18 years old.
/// </summary>
public class Over18Policy : IOver18Policy
{
    private readonly IUserRepository _userRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Over18Policy"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="dateTimeProvider">Injected service for time related functionality.</param>
    public Over18Policy(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _userRepository = unitOfWork.GetRepository<IUserRepository>();
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Evaluates the policy for the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for which to evaluate the policy.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the policy evaluation succeeds, <see langword="false"/> otherwise.</returns>
    public async Task<bool> EvaluateAsync(Guid userId, CancellationToken cancellationToken)
    {
        ErrorOr<UserEntity?> getUserResult = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsError || getUserResult.Value is null)
            return false;

        // TODO: uncomment when Date of Birth is implemented for users registration
        //if (!getUserResult.Value.DateOfBirth.HasValue)
        //    return false;

        int age = 18; // _dateTimeProvider.UtcNow.Year - getUserResult.Value.DateOfBirth.Value.Year;
        return age >= 18;
    }
}
