#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileManagement;
using Lumina.Application.Core.FileManagement.Paths.Commands.SplitPath;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Commands.SplitPath.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Fixtures;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Mapster;
using MapsterMapper;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Commands.SplitPath;

/// <summary>
/// Contains unit tests for the <see cref="SplitPathCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathCommandHandlerTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    private readonly IMapper _mockMapper;
    private readonly IPathService _mockPathService;
    private readonly SplitPathCommandHandler _sut;
    private readonly PathSegmentFixture _pathSegmentFixture;
    private readonly PathSegmentResponseFixture _pathSegmentResponseFixture;
    private readonly PathSegmentMappingConfig _pathSegmentMappingConfig;
    private readonly TypeAdapterConfig _config;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandHandlerTests"/> class.
    /// </summary>
    public SplitPathCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPathService = Substitute.For<IPathService>();
        _mockMapper = Substitute.For<IMapper>();
        _sut = new SplitPathCommandHandler(_mockPathService, _mockMapper);
        _pathSegmentFixture = new PathSegmentFixture();
        _pathSegmentResponseFixture = new PathSegmentResponseFixture();

        _config = new TypeAdapterConfig();
        _pathSegmentMappingConfig = new PathSegmentMappingConfig();
        _pathSegmentMappingConfig.Register(_config);
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task Handle_WhenCalledWithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        SplitPathCommand splitPathCommand = SplitPathCommandFixture.CreateSplitPathCommand();

        IEnumerable<PathSegment> pathSegments = _pathSegmentFixture.CreateMany();
        IEnumerable<PathSegmentResponse> pathSegmentResponses = _pathSegmentResponseFixture.CreateMany(pathSegments.Count());

        _mockPathService.ParsePath(splitPathCommand.Path)
            .Returns(ErrorOrFactory.From(pathSegments));

        _mockMapper.Map<IEnumerable<PathSegmentResponse>>(Arg.Any<IEnumerable<PathSegment>>())
            .Returns(pathSegmentResponses);

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.Handle(splitPathCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<PathSegmentResponse>>();
        result.Value.Should().BeEquivalentTo(pathSegmentResponses);
        _mockPathService.Received(1).ParsePath(splitPathCommand.Path);
        _mockMapper.Received(1).Map<IEnumerable<PathSegmentResponse>>(Arg.Any<IEnumerable<PathSegment>>());
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        SplitPathCommand command = _fixture.Create<SplitPathCommand>();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.ParsePath(command.Path)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockPathService.Received(1).ParsePath(command.Path);
        _mockMapper.DidNotReceive().Map<IEnumerable<PathSegmentResponse>>(Arg.Any<ErrorOr<IEnumerable<PathSegment>>>());
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        SplitPathCommand command = _fixture.Create<SplitPathCommand>();
        ErrorOr<IEnumerable<PathSegment>> emptyList = ErrorOrFactory.From(Enumerable.Empty<PathSegment>());
        _mockPathService.ParsePath(command.Path)
            .Returns(emptyList);

        _mockMapper.Map<IEnumerable<PathSegmentResponse>>(Arg.Any<IEnumerable<PathSegment>>())
            .Returns([]);

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockPathService.Received(1).ParsePath(command.Path);
        _mockMapper.Received(1).Map<IEnumerable<PathSegmentResponse>>(Arg.Any<IEnumerable<PathSegment>>());
    }
    #endregion
}
