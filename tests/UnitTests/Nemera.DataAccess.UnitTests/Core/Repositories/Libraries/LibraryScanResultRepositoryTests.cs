#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.DataAccess.Core.Repositories.Libraries;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.UnitTests.Core.Repositories.Libraries;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanResultRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanResultRepositoryTests
{
    private readonly NemeraDbContext _mockContext;
    private readonly LibraryScanResultRepository _sut;
    private readonly LibraryScanResultEntityFixture _libraryScanResultEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanResultRepositoryTests"/> class.
    /// </summary>
    public LibraryScanResultRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<NemeraDbContext>();
        _sut = new LibraryScanResultRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenCalled_ShouldAddScanResultToContextAndReturnCreated()
    {
        // Arrange
        LibraryScanResultEntity libraryScanResult = _libraryScanResultEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(libraryScanResult, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<LibraryScanResultEntity>? addedScanResult = _mockContext.ChangeTracker.Entries<LibraryScanResultEntity>()
            .FirstOrDefault(entityEntry => entityEntry.State == EntityState.Added && entityEntry.Entity.Id == libraryScanResult.Id);
        Assert.NotNull(addedScanResult);
    }
}
