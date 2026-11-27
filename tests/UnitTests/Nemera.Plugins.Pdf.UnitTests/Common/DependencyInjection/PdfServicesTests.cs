#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Reading;
using Lumina.Plugins.Pdf.Common.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Pdf.UnitTests.Common.DependencyInjection;

/// <summary>
/// Contains unit tests for the <see cref="PdfServices"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PdfServicesTests
{
    [Fact]
    public void AddPdfReader_WhenServicesIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddPdfReader(pluginId: Guid.NewGuid());

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void AddPdfReader_WhenCalled_ShouldReturnTheSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.AddPdfReader(pluginId: Guid.NewGuid());

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddPdfReader_WhenCalled_ShouldRegisterTheBookReaderForKeyedResolution()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddPdfReader(pluginId: pluginId);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetKeyedService<IBookReader>(pluginId));
        Assert.Null(serviceProvider.GetKeyedService<IBookReader>(Guid.NewGuid()));
    }
}
