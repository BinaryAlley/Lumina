#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;

/// <summary>
/// Interface for the repository for media library scan results.
/// </summary>
public interface ILibraryScanResultRepository : IRepository<LibraryScanResultEntity>,
                                                IInsertRepositoryAction<LibraryScanResultEntity>
{
}
