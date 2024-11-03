#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Drives.Queries.GetDrives;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Drives.Queries.GetDrives;

/// <summary>
/// Contains unit tests for the <see cref="GetDrivesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDrivesQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IDriveService _mockDriveService;
    private readonly GetDrivesQueryHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesQueryHandlerTests"/> class.
    /// </summary>
    public GetDrivesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockDriveService = Substitute.For<IDriveService>();
        _sut = new GetDrivesQueryHandler(_mockDriveService);
    }

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

        _mockDriveService.GetDrives()
            .Returns(ErrorOrFactory.From(drives));

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(getDrivesQuery, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>();
        result.Value.Should().HaveCount(drives.Count());

        List<FileSystemTreeNodeResponse> resultList = result.Value.ToList();
        List<FileSystemItem> drivesList = drives.ToList();

        for (int i = 0; i < resultList.Count; i++)
        {
            FileSystemTreeNodeResponse response = resultList[i];
            WindowsRootItem drive = (WindowsRootItem)drivesList[i];

            response.Path.Should().Be(drive.Id.Path);
            response.Name.Should().Be(drive.Name);
            response.ItemType.Should().Be(FileSystemItemType.Root);
            response.IsExpanded.Should().BeFalse();
            response.ChildrenLoaded.Should().BeFalse();
            response.Children.Should().BeEmpty();
        }

        _mockDriveService.Received(1).GetDrives();
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

        // Act
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockDriveService.Received(1).GetDrives();
    }
}
