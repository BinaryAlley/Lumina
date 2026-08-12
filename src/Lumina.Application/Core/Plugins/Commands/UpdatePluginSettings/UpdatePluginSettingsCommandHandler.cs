#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Mediator;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;

/// <summary>
/// Handler for the command to update the settings of a plugin.
/// </summary>
public class UpdatePluginSettingsCommandHandler : IRequestHandler<UpdatePluginSettingsCommand, ErrorOr<Success>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public UpdatePluginSettingsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the command to update the settings of a plugin.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async ValueTask<ErrorOr<Success>> Handle(UpdatePluginSettingsCommand request, CancellationToken cancellationToken)
    {
        IPluginRepository pluginRepository = _unitOfWork.GetRepository<IPluginRepository>();
        string? settingsJson = request.Settings is not null ? JsonSerializer.Serialize(request.Settings) : null;
        ErrorOr<Updated> updateResult = await pluginRepository.UpdateSettingsAsync(request.PluginId, settingsJson, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsError)
            return updateResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success;
    }
}
