#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Drives.Queries.GetDrives;
using Lumina.Application.UnitTests.Core.FileManagement.Drives.Queries.GetDrives.Fixtures;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using MapsterMapper;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Drives.Queries.GetDrives;

/// <summary>
/// Contains unit tests for the <see cref="GetDrivesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDrivesQueryHandlerTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    private readonly IMapper _mockMapper;
    private readonly IDriveService _mockDriveService;
    private readonly GetDrivesQueryHandler _sut;
    private readonly GetDrivesQueryFixture _getDrivesQueryFixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesQueryHandlerTests"/> class.
    /// </summary>
    public GetDrivesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockDriveService = Substitute.For<IDriveService>();
        _mockMapper = Substitute.For<IMapper>();
        _sut = new GetDrivesQueryHandler(_mockDriveService, _mockMapper);
        _getDrivesQueryFixture = new GetDrivesQueryFixture();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnSuccessResult()
    {
        // Arrange
        GetDrivesQuery getDrivesQuery = _fixture.Create<GetDrivesQuery>();

        IEnumerable<FileSystemItem> drives =
        [
            WindowsRootItem.Create("C:\\", "C:").Value,
            WindowsRootItem.Create("D:\\", "D:").Value
        ];

        List<FileSystemTreeNodeResponse> driveResponses = _getDrivesQueryFixture.CreateMany();

        _mockDriveService.GetDrives()
            .Returns(ErrorOrFactory.From(drives));

        _mockMapper.Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<FileSystemItem>>(d => d == drives))
            .Returns(driveResponses);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(getDrivesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().BeEquivalentTo(driveResponses);
        _mockDriveService.Received(1).GetDrives();
        _mockMapper.Received(1).Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Is<IEnumerable<FileSystemItem>>(d => d == drives));
    }

    [Fact]
    public async Task Handle_WhenDriveServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetDrivesQuery query = _fixture.Create<GetDrivesQuery>();
        Error error = Error.Failure("DriveService.Error", "An error occurred");
        _mockDriveService.GetDrives()
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockDriveService.Received(1).GetDrives();
    }

    [Fact]
    public async Task Handle_WhenDriveServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetDrivesQuery query = _fixture.Create<GetDrivesQuery>();
        ErrorOr<IEnumerable<FileSystemItem>> emptyList = ErrorOrFactory.From(Enumerable.Empty<FileSystemItem>());
        _mockDriveService.GetDrives()
            .Returns(emptyList);

        _mockMapper.Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Any<IEnumerable<FileSystemItem>>())
            .Returns([]);

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockDriveService.Received(1).GetDrives();
        _mockMapper.Received(1).Map<IEnumerable<FileSystemTreeNodeResponse>>(Arg.Any<IEnumerable<FileSystemItem>>());
    }
    #endregion
}
