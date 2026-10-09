#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Contains unit tests for the <see cref="BooksMediaLibraryScanMetadataEnricher"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BooksMediaLibraryScanMetadataEnricherTests
{
    private const string PROVIDER_A_NAME = "ProviderA";
    private const string PROVIDER_B_NAME = "ProviderB";

    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockMetadataConfigurationRepository;
    private readonly IUserSettingsRepository _mockUserSettingsRepository;
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IMetadataProvider _mockProviderA;
    private readonly IMetadataProvider _mockProviderB;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly ServiceProvider _serviceProvider;
    private readonly BooksMediaLibraryScanMetadataEnricher _sut;
    private readonly LibraryMetadataProviderConfigurationEntityFixture _metadataConfigurationEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookMetadataDtoFixture _bookMetadataDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly ScanId _scanId;
    private readonly UserId _userId;
    private readonly LibraryId _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksMediaLibraryScanMetadataEnricherTests"/> class.
    /// </summary>
    public BooksMediaLibraryScanMetadataEnricherTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockMetadataConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.LibraryMetadataProviderConfigurationRepository.Returns(_mockMetadataConfigurationRepository);
        _mockUnitOfWork.UserSettingsRepository.Returns(_mockUserSettingsRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);

        _mockProviderA = Substitute.For<IMetadataProvider>();
        ConfigureProvider(_mockProviderA, PROVIDER_A_NAME);
        _mockProviderB = Substitute.For<IMetadataProvider>();
        ConfigureProvider(_mockProviderB, PROVIDER_B_NAME);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);

        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _libraryId = _libraryIdFixture.Create();

        // The configured providers are registered under the plugin Ids they are configured with, the way the host registers plugin provided services.
        Guid pluginAId = Guid.NewGuid();
        Guid pluginBId = Guid.NewGuid();
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>.Success(
            [
                _metadataConfigurationEntityFixture.Create(_libraryId.Value, pluginAId, rank: 1),
                _metadataConfigurationEntityFixture.Create(_libraryId.Value, pluginBId, rank: 2)
            ]));

        ServiceCollection services = new();
        services.AddScoped(_ => _mockUnitOfWork);
        services.AddScoped(_ => _mockDomainEventPublisher);
        services.AddKeyedTransient(pluginAId, (serviceProvider, serviceKey) => _mockProviderA);
        services.AddKeyedTransient(pluginBId, (serviceProvider, serviceKey) => _mockProviderB);
        _serviceProvider = services.BuildServiceProvider();

        _sut = new BooksMediaLibraryScanMetadataEnricher(_serviceProvider.GetRequiredService<IServiceScopeFactory>(), NullLogger<BooksMediaLibraryScanMetadataEnricher>.Instance);

        _mockBookRepository.GetBooksNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _mockBookRepository.GetBooksNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BookEntity>>.Success([]));
        _mockUserSettingsRepository.GetByUserIdAsync(_userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<UserSettingsEntity?>.Success(null));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Success));
    }

    [Fact]
    public async Task EnrichAsync_WhenNoMetadataProviderIsConfigured_ShouldSkipTheEnrichment()
    {
        // Arrange
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>.Success([]));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockBookRepository.DidNotReceive().GetBooksNeedingMetadataCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheMetadataConfigurationsCannotBeRead_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockMetadataConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Provider.Error", "Failed to read the configurations"));

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheFirstProviderReturnsApplicableMetadata_ShouldNotQueryTheFollowingProviders()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableBookMetadata("Dune"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockProviderA.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockProviderB.DidNotReceive().GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
        Assert.Equal(PROVIDER_A_NAME, book.MetadataProvider);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnrichAsync_WhenTheFirstProviderReturnsNoUsableMetadata_ShouldQueryTheFollowingProviders()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)null);
        _mockProviderB.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableBookMetadata("Dune"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockProviderA.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockProviderB.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
        Assert.Equal(PROVIDER_B_NAME, book.MetadataProvider);
    }

    [Fact]
    public async Task EnrichAsync_WhenNoProviderReturnsUsableMetadata_ShouldMarkTheBookFailed()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)null);
        _mockProviderB.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)null);

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        Assert.Equal(MetadataStatus.Failed, book.MetadataStatus);
    }

    [Fact]
    public async Task EnrichAsync_WhenAggregationIsEnabled_ShouldQueryEveryProviderAndMergeTheirMetadata()
    {
        // Arrange
        _mockUserSettingsRepository.GetByUserIdAsync(_userId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<UserSettingsEntity?>.Success(_userSettingsEntityFixture.Create(userId: _userId.Value, shouldAggregateMetadataWhenMissing: true)));
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableBookMetadata("Dune"));
        _mockProviderB.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableBookMetadata("Dune Messiah"));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockProviderA.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await _mockProviderB.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
        Assert.Equal($"{PROVIDER_A_NAME}, {PROVIDER_B_NAME}", book.MetadataProvider);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheMetadataHasContributors_ShouldLinkThemToTheBook()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        MediaContributorEntity contributor = _mediaContributorEntityFixture.Create();
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(contributor));
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableBookMetadata(
                "Dune",
                contributors: [_mediaContributorDtoFixture.Create(displayName: "Frank Herbert", role: MediaContributorRole.Author, legalName: null)]));

        // Act
        await _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);

        // Assert
        await _mockMediaContributorRepository.Received(1).FindOrCreateByDisplayNameAsync("Frank Herbert", null, Arg.Any<CancellationToken>());
        BookContributorEntity linkedContributor = Assert.Single(book.Contributors);
        Assert.Equal(contributor.Id, linkedContributor.MediaContributorId);
        Assert.Equal(MediaContributorRole.Author, linkedContributor.Role);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheBookCountCannotBeRead_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockBookRepository.GetBooksNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to count the books"));

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheBooksPageCannotBeRead_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockBookRepository.GetBooksNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to read the books"));

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task EnrichAsync_WhenSavingTheChangesFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value);
        SetupBooks(book);
        _mockProviderA.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns((MetadataDto?)CreateApplicableBookMetadata("Dune"));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Repository.Error", "Failed to save the changes"));

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task EnrichAsync_WhenTheCancellationTokenIsAlreadyCancelled_ShouldCancelTheOperation()
    {
        // Arrange
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        Task Act()
        {
            return _sut.EnrichAsync(_libraryId, _scanId, _userId, cancellationTokenSource.Token);
        }

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
    }

    /// <summary>
    /// Stubs the book reader with a single page holding the provided books.
    /// </summary>
    /// <param name="books">The books of the page.</param>
    private void SetupBooks(params BookEntity[] books)
    {
        _mockBookRepository.GetBooksNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(books.Length));
        _mockBookRepository.GetBooksNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<IReadOnlyList<BookEntity>>.Success(books),
                Result<IReadOnlyList<BookEntity>>.Success([]));
    }

    /// <summary>
    /// Configures the provided substitute metadata provider so that it declares the book lookup type for the books media library type.
    /// </summary>
    /// <param name="metadataProvider">The metadata provider substitute to configure.</param>
    /// <param name="name">The display name the provider reports.</param>
    private static void ConfigureProvider(IMetadataProvider metadataProvider, string name)
    {
        metadataProvider.Name.Returns(name);
        metadataProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        metadataProvider.RequiresWebAccess.Returns(false);
        metadataProvider.LookupType.Returns(typeof(BookMetadataLookupDto));
    }

    /// <summary>
    /// Creates a book metadata that is usable and carries only a title, and optionally a set of contributors.
    /// </summary>
    /// <param name="title">The title of the book.</param>
    /// <param name="contributors">The optional contributors of the book.</param>
    /// <returns>The created book metadata.</returns>
    private BookMetadataDto CreateApplicableBookMetadata(string title, List<MediaContributorDto>? contributors = null)
    {
        return _bookMetadataDtoFixture.Create(
            title: title,
            contributors: contributors);
    }
}
