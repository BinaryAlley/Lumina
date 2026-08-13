#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Validation;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;

/// <summary>
/// Handler for the command to update the settings of a plugin.
/// </summary>
public class UpdatePluginSettingsCommandHandler : ICommandHandler<UpdatePluginSettingsCommand, ErrorOr<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdatePluginSettingsCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdatePluginSettingsCommandHandler(IUnitOfWork unitOfWork, IValidator<UpdatePluginSettingsCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update the settings of a plugin.
    /// </summary>
    /// <param name="command">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async Task<ErrorOr<Success>> HandleAsync(UpdatePluginSettingsCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        IPluginRepository pluginRepository = _unitOfWork.GetRepository<IPluginRepository>();
        string? settingsJson = command.Settings is not null ? JsonSerializer.Serialize(command.Settings) : null;
        ErrorOr<Updated> updateResult = await pluginRepository.UpdateSettingsAsync(command.PluginId, settingsJson, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsError)
            return updateResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success;
    }
}
