#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Seed;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Seed;

/// <summary>
/// Service for seeding initial data in the persistence medium.
/// </summary>
public class DataSeedService : IDataSeedService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSeedService"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param
    /// <param name="dateTimeProvider">Injected service for time related concerns.</param>
    public DataSeedService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Sets up default authorization permissions in the system.
    /// </summary>
    /// <param name="adminId">The unique identifier of the admin admin user who will own these permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> SetDefaultAuthorizationPermissionsAsync(Guid adminId, CancellationToken cancellationToken)
    {
        // Create the default authorization permissions and add them to the repository.
        PermissionEntity[] defaultPermissions =
        [
            new() { PermissionName = AuthorizationPermission.CanViewUsers, CreatedBy = adminId, CreatedOnUtc = _dateTimeProvider.UtcNow },
            new() { PermissionName = AuthorizationPermission.CanDeleteUsers, CreatedBy = adminId, CreatedOnUtc = _dateTimeProvider.UtcNow },
            new() { PermissionName = AuthorizationPermission.CanRegisterUsers, CreatedBy = adminId, CreatedOnUtc = _dateTimeProvider.UtcNow },
            new() { PermissionName = AuthorizationPermission.CanCreateLibraries, CreatedBy = adminId, CreatedOnUtc = _dateTimeProvider.UtcNow }
        ];
        foreach (PermissionEntity permission in defaultPermissions)
        {
            Result<Created> insertPermissionResult = await _unitOfWork.PermissionRepository.InsertAsync(permission, cancellationToken).ConfigureAwait(false);
            if (insertPermissionResult.IsFailure)
                return insertPermissionResult.Errors;
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Created;
    }

    /// <summary>
    /// Sets up default authorization roles in the system.
    /// </summary>
    /// <param name="userId">The unique identifier of the admin user for whom roles will be set.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> SetDefaultAuthorizationRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Create the default authorization roles and add them to the repository.
        RoleEntity[] defaultRoles =
        [
            new() { RoleName = "Admin", CreatedBy = userId, CreatedOnUtc = _dateTimeProvider.UtcNow }
        ];
        foreach (RoleEntity role in defaultRoles)
        {
            Result<Created> insertRoleResult = await _unitOfWork.RoleRepository.InsertAsync(role, cancellationToken).ConfigureAwait(false);
            if (insertRoleResult.IsFailure)
                return insertRoleResult.Errors;
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Created;
    }

    /// <summary>
    /// Assigns admin role permissions to the admin user.
    /// </summary>
    /// <param name="userId">The unique identifier of the admin user to receive admin role permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> SetAdminRolePermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Get the admin role.
        Result<RoleEntity?> getAdminRoleResult = await _unitOfWork.RoleRepository.GetByNameAsync("Admin", cancellationToken).ConfigureAwait(false);
        if (getAdminRoleResult.IsFailure)
            return getAdminRoleResult.Errors;

        if (getAdminRoleResult.Value is null)
            return Errors.Authorization.AdminAccountNotFound;

        // Get all permissions.
        Result<IEnumerable<PermissionEntity>> getPermissionsResult = await _unitOfWork.PermissionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getPermissionsResult.IsFailure)
            return getPermissionsResult.Errors;

        // Add each permission to the admin role.
        foreach (PermissionEntity permission in getPermissionsResult.Value)
        {
            RolePermissionEntity rolePermissionEntity = new()
            {
                Permission = permission,
                PermissionId = permission.Id,
                Role = getAdminRoleResult.Value,
                RoleId = getAdminRoleResult.Value.Id,
                CreatedBy = userId,
                CreatedOnUtc = _dateTimeProvider.UtcNow
            };
            Result<Created> insertRolePermissionResult = await _unitOfWork.RolePermissionRepository.InsertAsync(rolePermissionEntity, cancellationToken).ConfigureAwait(false);
            if (insertRolePermissionResult.IsFailure)
                return insertRolePermissionResult.Errors;
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Created;
    }

    /// <summary>
    /// Assigns admin role to the admin user.
    /// </summary>
    /// <param name="userId">The unique identifier of the admin user to receive the admin role.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> SetAdminRoleToAdministratorAccount(Guid userId, CancellationToken cancellationToken)
    {
        // Get the admin role.
        Result<RoleEntity?> getAdminRoleResult = await _unitOfWork.RoleRepository.GetByNameAsync("Admin", cancellationToken).ConfigureAwait(false);
        if (getAdminRoleResult.IsFailure)
            return getAdminRoleResult.Errors;

        if (getAdminRoleResult.Value is null)
            return Errors.Authorization.AdminRoleNotFound;

        // Get admin user.
        Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsFailure)
            return getUserResult.Errors;

        if (getUserResult.Value is null)
            return Errors.Authorization.AdminAccountNotFound;

        // Add the admin role to the admin user.
        UserRoleEntity userRole = new()
        {
            CreatedBy = userId,
            CreatedOnUtc = _dateTimeProvider.UtcNow,
            Role = getAdminRoleResult.Value,
            RoleId = getAdminRoleResult.Value.Id,
            User = getUserResult.Value,
            UserId = getUserResult.Value.Id
        };
        Result<Created> insertUserRoleResult = await _unitOfWork.UserRoleRepository.InsertAsync(userRole, cancellationToken).ConfigureAwait(false);
        if (insertUserRoleResult.IsFailure)
            return insertUserRoleResult.Errors;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Created;
    }

    /// <summary>
    /// Sets up the default scheduled jobs, owned by the administrator account.
    /// </summary>
    /// <param name="adminUserId">The unique identifier of the administrator account that owns the default scheduled jobs.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> SetDefaultScheduledJobsAsync(Guid adminUserId, CancellationToken cancellationToken)
    {
        // Seed the default jobs.
        ScheduledJobEntity[] defaultScheduledJobs =
        [
            // Scan all the enabled media libraries, daily at midnight.
            CreateDefaultScheduledJob("Daily media library scan", ScheduledTaskType.ScanMediaLibraries, ScheduleType.DailyAtHourAndMinute, intervalMinutes: null, hour: 0, minute: 0, adminUserId),
            // Clean the temporary directory into which the reading plugins extract the books, at every application startup.
            CreateDefaultScheduledJob("Clean temporary files at startup", ScheduledTaskType.CleanTemporaryFiles, ScheduleType.OnceAtStartup, intervalMinutes: null, hour: null, minute: null, adminUserId),
            // Clean the temporary directory into which the reading plugins extract the books, every 12 hours.
            CreateDefaultScheduledJob("Clean temporary files every 12 hours", ScheduledTaskType.CleanTemporaryFiles, ScheduleType.WithIntervalInMinutes, intervalMinutes: 720, hour: null, minute: null, adminUserId),
            // Repair the installed themes whose files are missing, at every application startup.
            CreateDefaultScheduledJob("Repair themes at startup", ScheduledTaskType.RepairThemes, ScheduleType.OnceAtStartup, intervalMinutes: null, hour: null, minute: null, adminUserId),
            // Clean the execution history of the scheduled jobs, keeping only the executions of the past month, at every application startup.
            CreateDefaultScheduledJob("Clean scheduled job execution history at startup", ScheduledTaskType.CleanScheduledJobExecutionHistory, ScheduleType.OnceAtStartup, intervalMinutes: null, hour: null, minute: null, adminUserId)
        ];
        foreach (ScheduledJobEntity defaultScheduledJob in defaultScheduledJobs)
        {
            Result<Created> insertScheduledJobResult = await _unitOfWork.ScheduledJobRepository.InsertAsync(defaultScheduledJob, cancellationToken).ConfigureAwait(false);
            if (insertScheduledJobResult.IsFailure)
                return insertScheduledJobResult.Errors;
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Created;
    }

    /// <summary>
    /// Creates a default scheduled job with its execution cycle active, owned by the administrator account identified by <paramref name="adminUserId"/>.
    /// </summary>
    /// <param name="name">The name of the default scheduled job.</param>
    /// <param name="taskType">The type of the task executed by the default scheduled job.</param>
    /// <param name="scheduleType">The type of the schedule of the default scheduled job.</param>
    /// <param name="intervalMinutes">The interval in minutes of the schedule, when it is an interval schedule.</param>
    /// <param name="hour">The hour of the schedule, when it is a daily schedule.</param>
    /// <param name="minute">The minute of the schedule, when it is a daily schedule.</param>
    /// <param name="adminUserId">The unique identifier of the administrator account that owns the default scheduled job.</param>
    /// <returns>The created default scheduled job.</returns>
    private ScheduledJobEntity CreateDefaultScheduledJob(string name, ScheduledTaskType taskType, ScheduleType scheduleType, int? intervalMinutes, int? hour, int? minute, Guid adminUserId)
    {
        return new ScheduledJobEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            TaskType = taskType,
            ScheduleType = scheduleType,
            IntervalMinutes = intervalMinutes,
            Hour = hour,
            Minute = minute,
            Status = ScheduledJobStatus.Active,
            OwnerUserId = adminUserId,
            LastStartedOnUtc = null,
            LastCompletedOnUtc = null,
            CreatedOnUtc = _dateTimeProvider.UtcNow,
            CreatedBy = adminUserId,
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
    }
}
