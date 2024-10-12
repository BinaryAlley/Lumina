#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Presentation.Api.Common.ModelBinders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.ModelBinders;

/// <summary>
/// Contains unit tests for the <see cref="UrlStringBinder"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UrlStringBinderTests
{
    private readonly UrlStringBinder _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlStringBinderTests"/> class.
    /// </summary>
    public UrlStringBinderTests()
    {
        _sut = new UrlStringBinder();
    }

    [Fact]
    public async Task BindModelAsync_WhenBindingContextIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        ModelBindingContext? bindingContext = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.BindModelAsync(bindingContext!));
    }

    [Fact]
    public async Task BindModelAsync_WhenValueProviderResultIsNone_ShouldReturnCompletedTask()
    {
        // Arrange
        ModelBindingContext mockBindingContext = Substitute.For<ModelBindingContext>();
        mockBindingContext.ValueProvider.GetValue(Arg.Any<string>()).Returns(ValueProviderResult.None);

        // Act
        await _sut.BindModelAsync(mockBindingContext);

        // Assert
        mockBindingContext.Result.Should().Be(ModelBindingResult.Failed());
    }

    [Fact]
    public async Task BindModelAsync_WhenFirstValueIsNull_ShouldSetFailedResult()
    {
        // Arrange
        ModelBindingContext mockBindingContext = Substitute.For<ModelBindingContext>();
        ValueProviderResult valueProviderResult = new(new StringValues((string?)null));
        mockBindingContext.ValueProvider.GetValue(Arg.Any<string>()).Returns(valueProviderResult);

        // Act
        await _sut.BindModelAsync(mockBindingContext);

        // Assert
        mockBindingContext.Result.Should().Be(ModelBindingResult.Failed());
    }

    [Fact]
    public async Task BindModelAsync_WhenFirstValueIsNullButValueProviderResultIsNotNone_ShouldSetFailedResult()
    {
        // Arrange
        ModelBindingContext mockBindingContext = Substitute.For<ModelBindingContext>();
        ValueProviderResult valueProviderResult = new(new StringValues([null]));
        mockBindingContext.ValueProvider.GetValue(Arg.Any<string>()).Returns(valueProviderResult);

        // Act
        await _sut.BindModelAsync(mockBindingContext);

        // Assert
        mockBindingContext.Result.Should().Be(ModelBindingResult.Failed());
    }

    [Fact]
    public async Task BindModelAsync_WhenValueIsProvided_ShouldDecodeAndSetSuccessResult()
    {
        // Arrange
        const string ENCODED_VALUE = "Hello%20World%21";
        const string DECODED_VALUE = "Hello World!";
        ModelBindingContext mockBindingContext = Substitute.For<ModelBindingContext>();
        mockBindingContext.ValueProvider.GetValue(Arg.Any<string>()).Returns(new ValueProviderResult(ENCODED_VALUE));

        // Act
        await _sut.BindModelAsync(mockBindingContext);

        // Assert
        mockBindingContext.Result.Should().Be(ModelBindingResult.Success(DECODED_VALUE));
    }

    [Fact]
    public async Task BindModelAsync_ShouldUseProvidedModelName()
    {
        // Arrange
        const string MODEL_NAME = "testModel";
        ModelBindingContext mockBindingContext = Substitute.For<ModelBindingContext>();
        mockBindingContext.ModelName.Returns(MODEL_NAME);

        // Act
        await _sut.BindModelAsync(mockBindingContext);

        // Assert
        mockBindingContext.ValueProvider.Received(1).GetValue(MODEL_NAME);
    }
}
