#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Contracts.Entities.MediaLibrary.Management;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;

/// <summary>
/// Interface for the repository for media libraries.
/// </summary>
public interface ILibraryRepository : IRepository<LibraryEntity>,
                                      IInsertRepositoryAction<LibraryEntity>
{
}
