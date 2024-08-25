#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Common.DependencyInjection;
using Lumina.Application.Common.DependencyInjection;
using Lumina.Infrastructure.Common.DependencyInjection;
#endregion

namespace Lumina.Presentation.Api;

/// <summary>
/// Application entry point, contains the composition root module, wires up all dependencies of the application.
/// </summary>
public class Program
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Optional command line arguments</param>
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.BindSharedConfiguration(builder.Configuration);
        builder.Services.BindPresentationApiLayerConfiguration(builder.Configuration);

        builder.Services.AddApplicationLayerServices();
        builder.Services.AddPresentationApiLayerServices();
        builder.Services.AddInfrastructureLayerServices();

        var app = builder.Build();

        app.UseExceptionHandler("/error"); // uses a middleware which reexecutes the request to the  error path

        // configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
    #endregion
}