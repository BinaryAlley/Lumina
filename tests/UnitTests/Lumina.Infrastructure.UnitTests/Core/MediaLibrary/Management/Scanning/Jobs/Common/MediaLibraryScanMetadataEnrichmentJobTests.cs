#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanMetadataEnrichmentJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanMetadataEnrichmentJobTests
{
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly IServiceScope _mockServiceScope;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryMetadataProviderConfigurationRepository _mockConfigurationRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly ILogger<MediaLibraryScanMetadataEnrichmentJob> _mockLogger;
    private readonly MediaLibraryScanMetadataEnrichmentJob _sut;
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly LibraryMetadataProviderConfigurationEntityFixture _configurationEntityFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly BookMetadataDtoFixture _bookMetadataDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ScanId _scanId;
    private readonly UserId _userId;
    private readonly LibraryId _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanMetadataEnrichmentJobTests"/> class.
    /// </summary>
    public MediaLibraryScanMetadataEnrichmentJobTests()
    {
        _mockServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _mockServiceScope = Substitute.For<IServiceScope>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceScopeFactory.CreateScope().Returns(_mockServiceScope);
        _mockServiceScope.ServiceProvider.Returns(_mockServiceProvider);

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConfigurationRepository = Substitute.For<ILibraryMetadataProviderConfigurationRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockUnitOfWork.LibraryMetadataProviderConfigurationRepository.Returns(_mockConfigurationRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);
        _mockServiceProvider.GetService(typeof(IUnitOfWork)).Returns(_mockUnitOfWork);

        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockDomainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _mockServiceProvider.GetService(typeof(IDomainEventPublisher)).Returns(_mockDomainEventPublisher);

        _mockLogger = Substitute.For<ILogger<MediaLibraryScanMetadataEnrichmentJob>>();

        _scanId = _scanIdFixture.Create();
        _userId = _userIdFixture.Create();
        _libraryId = _libraryIdFixture.Create();
        _sut = new MediaLibraryScanMetadataEnrichmentJob(_mockServiceScopeFactory, _mockLogger)
        {
            ScanId = _scanId,
            UserId = _userId,
            LibraryId = _libraryId
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoBooksNeedEnrichment_ShouldCompleteAndPublishFinishedEvent()
    {
        // Arrange
        _mockConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockBookRepository.GetBooksNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(0));
        _mockBookRepository.GetBooksNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFinishedDomainEvent>(domainEvent => domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoMetadataProviderIsConfigured_ShouldSkipEnrichmentWithoutMarkingBooksAsFailed()
    {
        // Arrange
        _mockConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // a library without metadata providers must not mark its books as failed to enrich, so the book repositories are not touched
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
        await _mockBookRepository.DidNotReceive().GetBooksNeedingMetadataCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetBooksNeedingMetadataAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenABookPageIsProcessed_ShouldDetachTheTrackedEntitiesAfterSaving()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "First Title"));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, shouldAggregateMetadataWhenMissing: false);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, firstPluginId);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the tracked entities of the page are detached after saving, so that the peak memory stays bounded regardless of the library size
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockUnitOfWork.Received(1).ClearTrackedEntities();
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGettingConfigurationsFails_ShouldMarkJobAsFailedAndPublishFailureEvent()
    {
        // Arrange
        _mockConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to get the metadata provider configurations"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Is<LibraryScanFailedDomainEvent>(domainEvent =>
            domainEvent.LibraryId == _libraryId
            && domainEvent.MediaLibraryScanCompositeId.ScanId == _scanId
            && domainEvent.MediaLibraryScanCompositeId.UserId == _userId), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldMarkJobAsCanceledAndThrow()
    {
        // Arrange
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        Task operation = _sut.ExecuteAsync(Guid.NewGuid(), new { }, cancellationTokenSource.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
        Assert.Equal(LibraryScanJobStatus.Canceled, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAggregateMetadataIsEnabled_ShouldFeedTheEarlierProvidersFindingsToTheLaterOnes()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        Guid secondPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        IMetadataProvider secondProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "First Title", openLibraryId: "OL123", description: "First Description"));
        secondProvider.Name.Returns("Second Provider");
        secondProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        secondProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "Second Title", publisher: "Second Publisher"));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, secondPluginId, secondProvider, shouldAggregateMetadataWhenMissing: true);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        // the book has no known identifiers of its own, so the findings of the first provider are fed to the second one
        book.OpenLibraryId = null;
        SetupSingleBookPage(book, firstPluginId, secondPluginId);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the second provider receives the lookup enriched with the open library Id discovered by the first provider
        await secondProvider.Received(1).GetMetadataAsync(
            Arg.Is<MetadataLookupDto>(lookup => lookup is BookMetadataLookupDto && ((BookMetadataLookupDto)lookup).OpenLibraryId == "OL123"),
            Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAggregateMetadataIsDisabled_ShouldOnlyApplyTheFirstUsableProviderMetadata()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        Guid secondPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        IMetadataProvider secondProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "First Title", publisher: "First Publisher"));
        secondProvider.Name.Returns("Second Provider");
        secondProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        secondProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "Second Title", publisher: "Second Publisher"));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, secondPluginId, secondProvider, shouldAggregateMetadataWhenMissing: false);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, firstPluginId, secondPluginId);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the second provider is not queried when the aggregation is disabled and the first provider returned usable metadata
        await secondProvider.DidNotReceive().GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoProviderReturnsUsableMetadata_ShouldMarkTheBookAsFailedToEnrich()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(includeTitle: false));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, shouldAggregateMetadataWhenMissing: true);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, firstPluginId);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the enrichment state is tracked directly on the entity, so the failed status is visible on the same instance
        Assert.Equal(MetadataStatus.Failed, book.MetadataStatus);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMetadataIsEnriched_ShouldSetTheEnrichmentTrackingColumnsOnTheEntity()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "First Title", publisher: "First Publisher"));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, shouldAggregateMetadataWhenMissing: false);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, firstPluginId);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the enrichment tracking columns are managed directly on the entity by the enrichment job
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
        Assert.Equal("First Provider", book.MetadataProvider);
        Assert.NotNull(book.LastMetadataUpdateUtc);
        Assert.Equal("First Title", book.Title);
        Assert.Equal("First Publisher", book.Publisher);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMetadataHasContributors_ShouldFindOrCreateThemAndLinkThemToTheBook()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        MediaContributorDto author = _mediaContributorDtoFixture.Create(displayName: "Frank Herbert", roleName: "Author", roleCategory: MediaContributorRoleCategory.Author);
        MediaContributorDto translator = _mediaContributorDtoFixture.Create(displayName: "Jane Translator", roleName: "Translator", roleCategory: MediaContributorRoleCategory.Translator);
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "First Title", contributors: [author, translator]));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, shouldAggregateMetadataWhenMissing: false);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, firstPluginId);

        MediaContributorEntity authorEntity = _mediaContributorEntityFixture.Create(displayName: "Frank Herbert");
        MediaContributorEntity translatorEntity = _mediaContributorEntityFixture.Create(displayName: "Jane Translator");
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync("Frank Herbert", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(authorEntity));
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync("Jane Translator", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(translatorEntity));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        await _mockMediaContributorRepository.Received(1).FindOrCreateByDisplayNameAsync("Frank Herbert", Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockMediaContributorRepository.Received(1).FindOrCreateByDisplayNameAsync("Jane Translator", Arg.Any<string?>(), Arg.Any<CancellationToken>());

        // the participation rows are replaced on the entity, carrying the role and the category of each contributor
        Assert.Equal(2, book.BookContributors.Count);
        BookContributorEntity linkedAuthor = Assert.Single(book.BookContributors, contributor => contributor.MediaContributorId == authorEntity.Id);
        Assert.Equal("Author", linkedAuthor.RoleName);
        Assert.Equal(MediaContributorRoleCategory.Author, linkedAuthor.RoleCategory);
        Assert.Equal(book.Id, linkedAuthor.BookId);
        BookContributorEntity linkedTranslator = Assert.Single(book.BookContributors, contributor => contributor.MediaContributorId == translatorEntity.Id);
        Assert.Equal("Translator", linkedTranslator.RoleName);
        Assert.Equal(MediaContributorRoleCategory.Translator, linkedTranslator.RoleCategory);
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheSameContributorAppearsInMultipleBooksOfAPage_ShouldFindOrCreateItOnlyOnce()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        MediaContributorDto author = _mediaContributorDtoFixture.Create(displayName: "Frank Herbert", roleName: "Author", roleCategory: MediaContributorRoleCategory.Author);
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "First Title", contributors: [author]));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, shouldAggregateMetadataWhenMissing: false);

        BookEntity firstBook = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/a.epub");
        BookEntity secondBook = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/b.epub");
        _mockConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([_configurationEntityFixture.Create(_libraryId.Value, firstPluginId, 1)]));
        _mockBookRepository.GetBooksNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(2));
        _mockBookRepository.GetBooksNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([firstBook, secondBook]), Result.From<IReadOnlyList<BookEntity>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        MediaContributorEntity authorEntity = _mediaContributorEntityFixture.Create(displayName: "Frank Herbert");
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync("Frank Herbert", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(authorEntity));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the contributor cache of the page guarantees a single contributor per person, so the repository is queried only once
        await _mockMediaContributorRepository.Received(1).FindOrCreateByDisplayNameAsync("Frank Herbert", Arg.Any<string?>(), Arg.Any<CancellationToken>());
        Assert.Single(firstBook.BookContributors);
        Assert.Single(secondBook.BookContributors);
        Assert.Equal(authorEntity.Id, firstBook.BookContributors[0].MediaContributorId);
        Assert.Equal(authorEntity.Id, secondBook.BookContributors[0].MediaContributorId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheLibraryForbidsWebDownloads_ShouldNotUseMetadataProvidersThatRequireWebAccess()
    {
        // Arrange
        Guid localPluginId = Guid.NewGuid();
        Guid webPluginId = Guid.NewGuid();
        IMetadataProvider localProvider = Substitute.For<IMetadataProvider>();
        localProvider.Name.Returns("Local Provider");
        localProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        localProvider.RequiresWebAccess.Returns(false);
        localProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(includeTitle: false));
        IMetadataProvider webProvider = Substitute.For<IMetadataProvider>();
        webProvider.Name.Returns("Web Provider");
        webProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        webProvider.RequiresWebAccess.Returns(true);
        webProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "Web Title"));

        SetupRealServiceProviderForProviders(localPluginId, localProvider, webPluginId, webProvider, shouldAggregateMetadataWhenMissing: false);
        ILibraryRepository mockLibraryRepository = Substitute.For<ILibraryRepository>();
        mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(title: "My Library", canDownloadMetadataFromWeb: false)));
        _mockUnitOfWork.LibraryRepository.Returns(mockLibraryRepository);

        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, localPluginId, webPluginId);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the local provider is used, but the provider requiring access to the web is skipped, so the book cannot be enriched
        await localProvider.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        await webProvider.DidNotReceive().GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheLibraryPermitsWebDownloads_ShouldUseMetadataProvidersThatRequireWebAccess()
    {
        // Arrange
        Guid localPluginId = Guid.NewGuid();
        Guid webPluginId = Guid.NewGuid();
        IMetadataProvider localProvider = Substitute.For<IMetadataProvider>();
        localProvider.Name.Returns("Local Provider");
        localProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        localProvider.RequiresWebAccess.Returns(false);
        localProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(includeTitle: false));
        IMetadataProvider webProvider = Substitute.For<IMetadataProvider>();
        webProvider.Name.Returns("Web Provider");
        webProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        webProvider.RequiresWebAccess.Returns(true);
        webProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "Web Title"));

        SetupRealServiceProviderForProviders(localPluginId, localProvider, webPluginId, webProvider, shouldAggregateMetadataWhenMissing: false);
        ILibraryRepository mockLibraryRepository = Substitute.For<ILibraryRepository>();
        mockLibraryRepository.GetByIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(_libraryEntityFixture.Create(title: "My Library", canDownloadMetadataFromWeb: true)));
        _mockUnitOfWork.LibraryRepository.Returns(mockLibraryRepository);

        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, localPluginId, webPluginId);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // the provider requiring access to the web is used when the library permits downloading data from the web
        await webProvider.Received(1).GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCountingTheBooksNeedingMetadataFails_ShouldMarkJobAsFailed()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, shouldAggregateMetadataWhenMissing: false);
        _mockConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([_configurationEntityFixture.Create(_libraryId.Value, firstPluginId, 1)]));
        _mockBookRepository.GetBooksNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to count the books to enrich"));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGettingTheBooksPageNeedingMetadataFails_ShouldMarkJobAsFailed()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, shouldAggregateMetadataWhenMissing: false);
        _mockConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([_configurationEntityFixture.Create(_libraryId.Value, firstPluginId, 1)]));
        _mockBookRepository.GetBooksNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockBookRepository.GetBooksNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to get the books to enrich"));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAMetadataProviderThrowsWhileTryingTheProvidersInOrder_ShouldTryTheNextProvider()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        Guid secondPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<MetadataDto?>(new InvalidOperationException("The metadata provider failed")));
        IMetadataProvider secondProvider = Substitute.For<IMetadataProvider>();
        secondProvider.Name.Returns("Second Provider");
        secondProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        secondProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "Second Title", publisher: "Second Publisher"));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, secondPluginId, secondProvider, shouldAggregateMetadataWhenMissing: false);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, firstPluginId, secondPluginId);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // a failing metadata provider must not prevent the metadata of the next provider from being used
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
        Assert.Equal("Second Provider", book.MetadataProvider);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAMetadataProviderThrowsWhileAggregatingMetadata_ShouldUseTheFindingsOfTheOtherProviders()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        Guid secondPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<MetadataDto?>(new InvalidOperationException("The metadata provider failed")));
        IMetadataProvider secondProvider = Substitute.For<IMetadataProvider>();
        secondProvider.Name.Returns("Second Provider");
        secondProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        secondProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "Second Title", publisher: "Second Publisher"));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, secondPluginId, secondProvider, shouldAggregateMetadataWhenMissing: true);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, firstPluginId, secondPluginId);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
        Assert.Equal("Second Provider", book.MetadataProvider);
        Assert.Equal("Second Title", book.Title);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAContributorHasNoDisplayName_ShouldSkipLinkingIt()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        MediaContributorDto namelessContributor = _mediaContributorDtoFixture.Create(roleName: "Author", roleCategory: MediaContributorRoleCategory.Author) with { Name = null };
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "First Title", contributors: [namelessContributor]));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, shouldAggregateMetadataWhenMissing: false);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, firstPluginId);

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        // a contributor without a display name cannot be linked to the book, so no contributor is queried or created
        await _mockMediaContributorRepository.DidNotReceive().FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        Assert.Empty(book.BookContributors);
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAContributorHasNoRole_ShouldLinkItWithTheDefaultRole()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        MediaContributorDto contributorWithoutRole = _mediaContributorDtoFixture.Create(displayName: "Frank Herbert") with { Role = null };
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "First Title", contributors: [contributorWithoutRole]));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, shouldAggregateMetadataWhenMissing: false);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub", includeMetadata: false);
        SetupSingleBookPage(book, firstPluginId);

        MediaContributorEntity authorEntity = _mediaContributorEntityFixture.Create(displayName: "Frank Herbert");
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync("Frank Herbert", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(authorEntity));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        BookContributorEntity linkedContributor = Assert.Single(book.BookContributors);
        Assert.Equal("Contributor", linkedContributor.RoleName);
        Assert.Equal(MediaContributorRoleCategory.Other, linkedContributor.RoleCategory);
        Assert.Equal(MetadataStatus.Enriched, book.MetadataStatus);
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFindingOrCreatingAContributorFails_ShouldMarkJobAsFailed()
    {
        // Arrange
        Guid firstPluginId = Guid.NewGuid();
        IMetadataProvider firstProvider = Substitute.For<IMetadataProvider>();
        firstProvider.Name.Returns("First Provider");
        firstProvider.SupportedLibraryTypes.Returns([LibraryType.Book]);
        MediaContributorDto author = _mediaContributorDtoFixture.Create(displayName: "Frank Herbert", roleName: "Author", roleCategory: MediaContributorRoleCategory.Author);
        firstProvider.GetMetadataAsync(Arg.Any<MetadataLookupDto>(), Arg.Any<CancellationToken>())
            .Returns(_bookMetadataDtoFixture.Create(title: "First Title", contributors: [author]));

        SetupRealServiceProviderForProviders(firstPluginId, firstProvider, shouldAggregateMetadataWhenMissing: false);
        BookEntity book = _bookEntityFixture.Create(libraryId: _libraryId.Value, path: "/books/test.epub");
        SetupSingleBookPage(book, firstPluginId);

        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync("Frank Herbert", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Database.Error", "Failed to find or create the contributor"));

        // Act
        await _sut.ExecuteAsync(Guid.NewGuid(), new { }, CancellationToken.None);

        // Assert
        Assert.Equal(LibraryScanJobStatus.Failed, _sut.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheJobHasChildJobs_ShouldExecuteThemAfterCompletingItsPayload()
    {
        // Arrange
        _mockConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        IMediaLibraryScanJob mockChildJob = Substitute.For<IMediaLibraryScanJob>();
        mockChildJob.ExecuteAsync(Arg.Any<Guid>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _sut.AddChild(mockChildJob);
        Guid id = Guid.NewGuid();
        object input = new();

        // Act
        await _sut.ExecuteAsync(id, input, CancellationToken.None);

        // Assert
        await mockChildJob.Received(1).ExecuteAsync(id, input, Arg.Any<CancellationToken>());
        Assert.Equal(LibraryScanJobStatus.Completed, _sut.Status);
    }

    /// <summary>
    /// Wires a real service provider with the given metadata providers, so that the job resolves them from the dependency injection container.
    /// </summary>
    /// <param name="firstPluginId">The Id of the plugin providing the first metadata provider.</param>
    /// <param name="firstProvider">The first metadata provider to resolve.</param>
    /// <param name="secondPluginId">The Id of the plugin providing the second metadata provider, if any.</param>
    /// <param name="secondProvider">The second metadata provider to resolve, if any.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the user settings enable aggregating metadata from multiple providers when fields are missing.</param>
    private void SetupRealServiceProviderForProviders(Guid firstPluginId, IMetadataProvider firstProvider, Guid? secondPluginId = null, IMetadataProvider? secondProvider = null, bool shouldAggregateMetadataWhenMissing = false)
    {
        ServiceCollection services = new();
        services.AddKeyedSingleton(firstPluginId, firstProvider);
        if (secondPluginId is not null && secondProvider is not null)
            services.AddKeyedSingleton(secondPluginId.Value, secondProvider);
        services.AddSingleton(_mockUnitOfWork);
        services.AddSingleton(_mockDomainEventPublisher);
        // the provider is intentionally not disposed here, so that the async service scope used by the job stays alive for the whole test
        ServiceProvider realServiceProvider = services.BuildServiceProvider();
        AsyncServiceScope asyncServiceScope = realServiceProvider.CreateAsyncScope();
        _mockServiceScopeFactory.CreateAsyncScope().Returns(asyncServiceScope);

        IUserSettingsRepository mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();
        mockUserSettingsRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<UserSettingsEntity?>(_userSettingsEntityFixture.Create(shouldAggregateMetadataWhenMissing: shouldAggregateMetadataWhenMissing)));
        _mockUnitOfWork.UserSettingsRepository.Returns(mockUserSettingsRepository);
    }

    /// <summary>
    /// Stubs the repositories so that the job processes a single page containing the given book, configured with the given metadata providers.
    /// </summary>
    /// <param name="book">The book the job must enrich.</param>
    /// <param name="configuredPluginIds">The Ids of the plugins configured as metadata providers for the library.</param>
    private void SetupSingleBookPage(BookEntity book, params Guid[] configuredPluginIds)
    {
        _mockConfigurationRepository.GetByLibraryIdAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>(
                configuredPluginIds.Select((pluginId, index) => _configurationEntityFixture.Create(_libraryId.Value, pluginId, index + 1)).ToList()));
        _mockBookRepository.GetBooksNeedingMetadataCountAsync(_libraryId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From(1));
        _mockBookRepository.GetBooksNeedingMetadataAsync(_libraryId.Value, Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([book]), Result.From<IReadOnlyList<BookEntity>>([]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }
}
