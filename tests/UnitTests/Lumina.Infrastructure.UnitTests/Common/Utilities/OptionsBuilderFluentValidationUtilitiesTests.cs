#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
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
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsBuilderFluentValidationUtilitiesTests"/> class.
    /// </summary>
    public OptionsBuilderFluentValidationUtilitiesTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void ValidateFluently_WhenCalled_ShouldRegisterFluentValidationOptions()
    {
        // Arrange
        var services = Substitute.For<IServiceCollection>();
        var name = _fixture.Create<string>();
        var optionsBuilder = new OptionsBuilder<OptionsBuilderFluentValidationUtilitiesFixture>(services, name);

        // Act
        var result = optionsBuilder.ValidateFluently();

        // Assert
        result.Should().BeSameAs(optionsBuilder);
        services.Received(1).Add(Arg.Is<ServiceDescriptor>(sd =>
            sd.ServiceType == typeof(IValidateOptions<OptionsBuilderFluentValidationUtilitiesFixture>) &&
            sd.Lifetime == ServiceLifetime.Singleton &&
            sd.ImplementationFactory != null));
    }

    [Fact]
    public void ValidateFluently_WhenCalled_ShouldUseCorrectName()
    {
        // Arrange
        var services = new ServiceCollection();
        var name = _fixture.Create<string>();
        var optionsBuilder = new OptionsBuilder<OptionsBuilderFluentValidationUtilitiesFixture>(services, name);

        // Act
        optionsBuilder.ValidateFluently();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(IValidateOptions<OptionsBuilderFluentValidationUtilitiesFixture>) &&
            sd.Lifetime == ServiceLifetime.Singleton &&
            sd.ImplementationFactory != null);

        serviceDescriptor.Should().NotBeNull();

        var implementationFactory = serviceDescriptor!.ImplementationFactory;
        implementationFactory.Should().NotBeNull();

        var serviceProvider = Substitute.For<IServiceProvider>();
        var mockValidator = Substitute.For<IValidator<OptionsBuilderFluentValidationUtilitiesFixture>>();
        serviceProvider.GetService(typeof(IValidator<OptionsBuilderFluentValidationUtilitiesFixture>))
            .Returns(mockValidator);

        var fluentValidationOptions = implementationFactory!(serviceProvider) as FluentValidationOptions<OptionsBuilderFluentValidationUtilitiesFixture>;
        fluentValidationOptions.Should().NotBeNull();
        fluentValidationOptions!.Name.Should().Be(name);
    }
    #endregion
}