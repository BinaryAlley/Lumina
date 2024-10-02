#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web;

/// <summary>
/// Application entry point, contains the composition root module, wires up all dependencies of the application.
/// </summary>
public class Program
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Optional command line arguments.</param>
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.BindConfiguration(builder.Configuration);
        builder.Services.AddPresentationWebLayerServices();

        WebApplication app = builder.Build();

        // configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        await app.RunAsync();
    }
    #endregion
}
