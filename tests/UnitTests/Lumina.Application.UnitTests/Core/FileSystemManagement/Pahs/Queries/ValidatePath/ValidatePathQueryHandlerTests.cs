#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.ValidatePath.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.ValidatePath;

/// <summary>
/// Contains unit tests for the <see cref="ValidatePathQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathQueryHandlerTests
{
    private readonly IPathService _mockPathService;
    private readonly ValidatePathQueryHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathQueryHandlerTests"/> class.
    /// </summary>
    public ValidatePathQueryHandlerTests()
    {
        _mockPathService = Substitute.For<IPathService>();
        _sut = new ValidatePathQueryHandler(_mockPathService);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidPath_ShouldReturnTrueResponse()
    {
        // Arrange
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
        _mockPathService.IsValidPath(query.Path!).Returns(true);

        // Act
        PathValidResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        _mockPathService.Received(1).IsValidPath(query.Path!);
    }

    [Fact]
    public async Task Handle_WhenCalledWithInvalidPath_ShouldReturnFalseResponse()
    {
        // Arrange
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
        _mockPathService.IsValidPath(query.Path!).Returns(false);

        // Act
        PathValidResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        _mockPathService.Received(1).IsValidPath(query.Path!);
    }

    [Fact]
    public async Task Handle_WhenCalledWithNullPath_ShouldReturnFalseResponse()
    {
        // Arrange
        ValidatePathQuery query = new(null!);
        _mockPathService.IsValidPath(Arg.Any<string>()).Returns(false);

        // Act
        PathValidResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        _mockPathService.Received(1).IsValidPath(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenCalledWithEmptyPath_ShouldReturnFalseResponse()
    {
        // Arrange
        ValidatePathQuery query = new(string.Empty);
        _mockPathService.IsValidPath(Arg.Any<string>()).Returns(false);

        // Act
        PathValidResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        _mockPathService.Received(1).IsValidPath(Arg.Any<string>());
    }
}
