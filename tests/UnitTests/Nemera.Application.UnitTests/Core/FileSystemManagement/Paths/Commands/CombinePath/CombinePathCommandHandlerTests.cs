#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;
using Nemera.Application.Fixtures.Core.FileSystemManagement.Paths.Commands.CombinePath;
using Nemera.Contracts.Responses.FileSystemManagement.Path;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.UnitTests.Core.FileSystemManagement.Paths.Commands.CombinePath;

/// <summary>
/// Contains unit tests for the <see cref="CombinePathCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IPathService _mockPathService;
    private readonly CombinePathCommandHandler _sut;
    private readonly CombinePathCommandFixture _combinePathCommandFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathCommandHandlerTests"/> class.
    /// </summary>
    public CombinePathCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPathService = Substitute.For<IPathService>();
        IValidator<CombinePathCommand> mockValidator = Substitute.For<IValidator<CombinePathCommand>>();
        mockValidator.Validate(Arg.Any<CombinePathCommand>())
            .Returns([]);
        _sut = new CombinePathCommandHandler(_mockPathService, mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        CombinePathCommand combinePathCommand = _combinePathCommandFixture.Create();
        string combinedPath = System.IO.Path.Combine(combinePathCommand.OriginalPath!, combinePathCommand.NewPath!);

        _mockPathService.CombinePath(combinePathCommand.OriginalPath!, combinePathCommand.NewPath!)
            .Returns(Result.From(combinedPath));

        // Act
        Result<PathSegmentResponse> result = await _sut.HandleAsync(combinePathCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.IsType<PathSegmentResponse>(result.Value);
        Assert.Equal(combinedPath, result.Value.Path);
        _mockPathService.Received(1).CombinePath(combinePathCommand.OriginalPath!, combinePathCommand.NewPath!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        CombinePathCommand command = _combinePathCommandFixture.Create();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.CombinePath(command.OriginalPath!, command.NewPath!)
            .Returns(error);

        // Act
        Result<PathSegmentResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockPathService.Received(1).CombinePath(command.OriginalPath!, command.NewPath!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathServiceReturnsEmptyString_ShouldReturnSuccessResultWithEmptyPath()
    {
        // Arrange
        CombinePathCommand command = _combinePathCommandFixture.Create();
        _mockPathService.CombinePath(command.OriginalPath!, command.NewPath!)
            .Returns(Result.From(string.Empty));

        // Act
        Result<PathSegmentResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.IsType<PathSegmentResponse>(result.Value);
        Assert.Empty(result.Value.Path);
        _mockPathService.Received(1).CombinePath(command.OriginalPath!, command.NewPath!);
    }
}
