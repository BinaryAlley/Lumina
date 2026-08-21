#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Helpers shared by the theme integration tests.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ThemeTestHelpers
{
    /// <summary>
    /// Waits until the bundled 'editorial-paper' theme has been installed and activated by the theme detection job at startup.
    /// </summary>
    /// <param name="apiFactory">The shared API factory whose database is polled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WaitForBundledThemeAsync(AuthenticatedLuminaApiFactory apiFactory)
    {
        const int MAX_ATTEMPTS = 100;
        const int DELAY_MILLISECONDS = 100;

        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            using IServiceScope scope = apiFactory.Services.CreateScope();
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

            if (await dbContext.Themes.AnyAsync(theme => theme.ThemeId == "editorial-paper" && theme.IsCurrent == true))
                return;

            await Task.Delay(DELAY_MILLISECONDS);
        }

        Assert.Fail("The bundled theme 'editorial-paper' was not installed and activated within the expected time.");
    }
}
