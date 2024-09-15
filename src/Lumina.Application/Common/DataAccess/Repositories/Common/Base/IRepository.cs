#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Models.Common;

#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Common.Base;

/// <summary>
/// Interface for interaction with a generic persistance medium.
/// </summary>
/// <typeparam name="TModel">The type used for the repository. It should implement <see cref="IStorageEntity"/>.</typeparam>
public interface IRepository<TModel> where TModel : IStorageEntity
{
}