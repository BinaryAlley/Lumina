#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Lumina.Application.Fixtures.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Paths.Commands.SplitPath;

/// <summary>
/// Contains unit tests for the <see cref="SplitPathCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IPathService _mockPathService;
    private readonly SplitPathCommandHandler _sut;
    private readonly PathSegmentFixture _pathSegmentFixture;
    private readonly SplitPathCommandFixture _splitPathCommandFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandHandlerTests"/> class.
    /// </summary>
    public SplitPathCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPathService = Substitute.For<IPathService>();
        IValidator<SplitPathCommand> mockValidator = Substitute.For<IValidator<SplitPathCommand>>();
        mockValidator.Validate(Arg.Any<SplitPathCommand>())
            .Returns([]);
        _sut = new SplitPathCommandHandler(_mockPathService, mockValidator);
        _pathSegmentFixture = new PathSegmentFixture();
        _splitPathCommandFixture = new SplitPathCommandFixture();
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        SplitPathCommand splitPathCommand = _splitPathCommandFixture.Create();

        IEnumerable<PathSegment> pathSegments = _pathSegmentFixture.CreateMany();

        _mockPathService.ParsePath(splitPathCommand.Path!)
            .Returns(Result.From(pathSegments));

        // Act
        Result<IEnumerable<PathSegmentResponse>> result = await _sut.HandleAsync(splitPathCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.IsAssignableFrom<IEnumerable<PathSegmentResponse>>(result.Value);
        Assert.Equal(pathSegments.Count(), result.Value.Count());

        List<PathSegmentResponse> resultList = [.. result.Value];
        List<PathSegment> segmentsList = [.. pathSegments];

        for (int i = 0; i < resultList.Count; i++)
            Assert.Equal(segmentsList[i].Name, resultList[i].Path);
        _mockPathService.Received(1).ParsePath(splitPathCommand.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        SplitPathCommand command = _splitPathCommandFixture.Create();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.ParsePath(command.Path!)
            .Returns(error);

        // Act
        Result<IEnumerable<PathSegmentResponse>> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockPathService.Received(1).ParsePath(command.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        SplitPathCommand command = _splitPathCommandFixture.Create();
        Result<IEnumerable<PathSegment>> emptyList = Result.From(Enumerable.Empty<PathSegment>());
        _mockPathService.ParsePath(command.Path!)
            .Returns(emptyList);

        // Act
        Result<IEnumerable<PathSegmentResponse>> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
        _mockPathService.Received(1).ParsePath(command.Path!);
    }
}
