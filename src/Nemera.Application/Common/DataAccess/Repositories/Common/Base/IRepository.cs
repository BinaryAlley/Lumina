#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Common;
#endregion

namespace Nemera.Application.Common.DataAccess.Repositories.Common.Base;

/// <summary>
/// Interface for interaction with a generic persistence medium.
/// </summary>
/// <typeparam name="TModel">The type used for the repository. It should implement <see cref="IStorageEntity"/>.</typeparam>
public interface IRepository<TModel> where TModel : IStorageEntity
{
}
