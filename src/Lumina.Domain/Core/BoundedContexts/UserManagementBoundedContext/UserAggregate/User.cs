#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate;

/// <summary>
/// Aggregate root for a user aggregate.
/// </summary>
[DebuggerDisplay("{Id}: {Username}")]
public sealed class User : AggregateRoot<UserId>
{
    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="username">The username of the user.</param>
    private User(
        UserId id,
        string username) : base(id)
    {
        Username = username;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="User"/>, or an error message.
    /// </returns>
    public static ErrorOr<User> Create(
        string username)
    {
        return new User(
            UserId.CreateUnique(),
            username);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="User"/>, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the id of the user.</param>
    /// <param name="username">The username of the user.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="User"/>, or an error message.
    /// </returns>
    public static ErrorOr<User> Create(
        UserId id,
        string username)
    {
        return new User(
            id,
            username);
    }
}
