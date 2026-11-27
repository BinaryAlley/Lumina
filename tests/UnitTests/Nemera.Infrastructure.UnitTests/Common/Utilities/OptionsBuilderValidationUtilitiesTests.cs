#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Infrastructure.Common.Utilities;
using Lumina.Infrastructure.Common.Validation;
using Lumina.Infrastructure.Fixtures.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="OptionsBuilderValidationUtilities"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OptionsBuilderValidationUtilitiesTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsBuilderValidationUtilitiesTests"/> class.
    /// </summary>
    public OptionsBuilderValidationUtilitiesTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ValidateFluently_WhenCalled_ShouldRegisterFluentValidationOptions()
    {
        // Arrange
        IServiceCollection services = Substitute.For<IServiceCollection>();
        string name = _fixture.Create<string>();
        OptionsBuilder<OptionsBuilderValidationUtilitiesFixture> optionsBuilder = new(services, name);

        // Act
        OptionsBuilder<OptionsBuilderValidationUtilitiesFixture> result = optionsBuilder.ValidateFluently();

        // Assert
        Assert.Same(optionsBuilder, result);
        services.Received(1).Add(Arg.Is<ServiceDescriptor>(sd =>
            sd.ServiceType == typeof(IValidateOptions<OptionsBuilderValidationUtilitiesFixture>) &&
            sd.Lifetime == ServiceLifetime.Singleton &&
            sd.ImplementationFactory != null));
    }

    [Fact]
    public void ValidateFluently_WhenCalled_ShouldUseCorrectName()
    {
        // Arrange
        ServiceCollection services = new();
        string name = _fixture.Create<string>();
        OptionsBuilder<OptionsBuilderValidationUtilitiesFixture> optionsBuilder = new(services, name);

        // Act
        optionsBuilder.ValidateFluently();

        // Assert
        ServiceDescriptor? serviceDescriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(IValidateOptions<OptionsBuilderValidationUtilitiesFixture>) &&
            sd.Lifetime == ServiceLifetime.Singleton &&
            sd.ImplementationFactory != null);

        Assert.NotNull(serviceDescriptor);

        Func<IServiceProvider, object>? implementationFactory = serviceDescriptor!.ImplementationFactory;
        Assert.NotNull(implementationFactory);

        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        IValidator<OptionsBuilderValidationUtilitiesFixture> mockValidator = Substitute.For<IValidator<OptionsBuilderValidationUtilitiesFixture>>();
        serviceProvider.GetService(typeof(IValidator<OptionsBuilderValidationUtilitiesFixture>))
            .Returns(mockValidator);

        ValidationOptions<OptionsBuilderValidationUtilitiesFixture>? validationOptions = implementationFactory!(serviceProvider) as ValidationOptions<OptionsBuilderValidationUtilitiesFixture>;
        Assert.NotNull(validationOptions);
        Assert.Equal(name, validationOptions.Name);
    }
}
