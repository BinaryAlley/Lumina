#region ========================================================================= USING =====================================================================================
using Nemera.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Helpers shared by the theme integration tests.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ThemeTestHelpers
{
    /// <summary>
    /// Waits until the bundled 'nemera-default' theme has been installed and activated by the theme detection job at startup.
    /// </summary>
    /// <param name="apiFactory">The shared API factory whose database is polled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WaitForBundledThemeAsync(AuthenticatedNemeraApiFactory apiFactory)
    {
        const int MAX_ATTEMPTS = 100;
        const int DELAY_MILLISECONDS = 100;

        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            using IServiceScope scope = apiFactory.Services.CreateScope();
            NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();

            if (await dbContext.Themes.AnyAsync(theme => theme.ThemeId == "nemera-default" && theme.IsCurrent == true))
                return;

            await Task.Delay(DELAY_MILLISECONDS);
        }

        Assert.Fail("The bundled theme 'nemera-default' was not installed and activated within the expected time.");
    }
}
