#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentValidation;
using Lumina.Infrastructure.Common.Utilities;
using Lumina.Infrastructure.Common.Validation;
using Lumina.Infrastructure.UnitTests.Common.Utilities.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="OptionsBuilderFluentValidationUtilities"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OptionsBuilderFluentValidationUtilitiesTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsBuilderFluentValidationUtilitiesTests"/> class.
    /// </summary>
    public OptionsBuilderFluentValidationUtilitiesTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ValidateFluently_WhenCalled_ShouldRegisterFluentValidationOptions()
    {
        // Arrange
        IServiceCollection services = Substitute.For<IServiceCollection>();
        string name = _fixture.Create<string>();
        OptionsBuilder<OptionsBuilderFluentValidationUtilitiesFixture> optionsBuilder = new(services, name);

        // Act
        OptionsBuilder<OptionsBuilderFluentValidationUtilitiesFixture> result = optionsBuilder.ValidateFluently();

        // Assert
        Assert.Same(optionsBuilder, result);
        services.Received(1).Add(Arg.Is<ServiceDescriptor>(sd =>
            sd.ServiceType == typeof(IValidateOptions<OptionsBuilderFluentValidationUtilitiesFixture>) &&
            sd.Lifetime == ServiceLifetime.Singleton &&
            sd.ImplementationFactory != null));
    }

    [Fact]
    public void ValidateFluently_WhenCalled_ShouldUseCorrectName()
    {
        // Arrange
        ServiceCollection services = new();
        string name = _fixture.Create<string>();
        OptionsBuilder<OptionsBuilderFluentValidationUtilitiesFixture> optionsBuilder = new(services, name);

        // Act
        optionsBuilder.ValidateFluently();

        // Assert
        ServiceDescriptor? serviceDescriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(IValidateOptions<OptionsBuilderFluentValidationUtilitiesFixture>) &&
            sd.Lifetime == ServiceLifetime.Singleton &&
            sd.ImplementationFactory != null);

        Assert.NotNull(serviceDescriptor);

        Func<IServiceProvider, object>? implementationFactory = serviceDescriptor!.ImplementationFactory;
        Assert.NotNull(implementationFactory);

        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        IValidator<OptionsBuilderFluentValidationUtilitiesFixture> mockValidator = Substitute.For<IValidator<OptionsBuilderFluentValidationUtilitiesFixture>>();
        serviceProvider.GetService(typeof(IValidator<OptionsBuilderFluentValidationUtilitiesFixture>))
            .Returns(mockValidator);

        FluentValidationOptions<OptionsBuilderFluentValidationUtilitiesFixture>? fluentValidationOptions = implementationFactory!(serviceProvider) as FluentValidationOptions<OptionsBuilderFluentValidationUtilitiesFixture>;
        Assert.NotNull(fluentValidationOptions);
        Assert.Equal(name, fluentValidationOptions.Name);
    }
}
