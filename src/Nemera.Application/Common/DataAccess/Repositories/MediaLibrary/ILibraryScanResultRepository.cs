#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Repositories.Common.Actions;
using Nemera.Application.Common.DataAccess.Repositories.Common.Base;
#endregion

namespace Nemera.Application.Common.DataAccess.Repositories.MediaLibrary;

/// <summary>
/// Interface for the repository for media library scan results.
/// </summary>
public interface ILibraryScanResultRepository : IRepository<LibraryScanResultEntity>,
                                                IInsertRepositoryAction<LibraryScanResultEntity>
{
}
