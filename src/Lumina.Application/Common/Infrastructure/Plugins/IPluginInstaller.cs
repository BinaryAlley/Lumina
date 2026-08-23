#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Infrastructure.Plugins;

/// <summary>
/// Service for installing plugins from an uploaded archive into the plugin storage directory.
/// </summary>
public interface IPluginInstaller
{
    /// <summary>
    /// Installs the plugin from the provided archive, placing its assemblies into the plugin storage directory.
    /// The installed plugin is loaded by the host application at the next startup.
    /// </summary>
    /// <param name="archive">The archive stream of the uploaded plugin.</param>
    /// <param name="fileName">The file name of the uploaded plugin.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> InstallAsync(Stream archive, string fileName, CancellationToken cancellationToken);
}
