# Architecture Decision Record (ADR): 0001 - Use of `IServiceScopeFactory` for EF Core Lifetime Management in Library Scan Background Jobs

## Status
**Accepted** (2025-02-15)

## Context
The media library scanning system requires complex job graphs with parent-child relationships that cannot be recreated during execution. The conflict arises from:

1. **Complex creational pattern**: Library scan jobs have complex parent-child dynamic relationships that needs to be established during the initiation of a library scan. This complex creational logic requires encapsulation (see `IMediaTypeScanner.CreateScanJobsForLibrary()`).
2. **Special job execution treatment**: Only top level library scan jobs with no parents are placed on an in-memory queue, from where they will be processed. The children are not placed in this queue because their processing depends on the payload(s) delivered by their parent(s). Therefor, it is the responsibility of the parents to fire the execution of their children's jobs, when their processing is done, and not by the background queue processing job.
3. **Discrepancy between user scope and background processing scope**: Processing of queued jobs is done in a singleton context, due to some jobs being long running processes that need to run in a background service. This means that any scoped services created when the user triggered the library scan are long disposed when child jobs are triggered by their parents. This includes unit of works, repositories and DbContexts.
1. **EF Core Lifetime Requirements**: DbContext (scoped) becomes disposed before background jobs execute.
2. **Domain Integrity Constraints**: Jobs are first-class domain objects with established relationships that must persist.
3. **Architecture Layer Boundaries**: Application layer must not reference EF Core directly.

## Decission
`IServiceScopeFactory` will be used in a controlled manner to create execution contexts for background jobs:

```chsarp
// sample job class (simplified)
internal sealed class FileSystemDiscoveryJob : MediaLibraryScanJob, IFileSystemDiscoveryJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    protected FileSystemDiscoveryJob(IServiceScopeFactory scopeFactory) 
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ExecuteAsync() 
    {
        await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
        IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>();
        ILibraryRepository libraryRepository = unitOfWork.GetRepository<ILibraryRepository>();
        // execution logic
    }
}
```
## Consequences

### Positive Outcomes

| Aspect                | Benefit                                                   |
|-----------------------|-----------------------------------------------------------|
| Lifetime Management   | Ensures fresh DbContext per job execution                 |
| Domain Model          | Preserves job graph relationships                         |
| Performance           | Enables parallel processing without shared state          |
| Layer Integrity       | Maintains application layer purity                        |

### Risks and Tradeoffs

| Risk                      | Mitigation Strategy                                               |
|---------------------------|-------------------------------------------------------------------|
| Service Locator Pattern   | Strictly limit usage to job classes by marking them as `sealed`   |
| Scope Leakage             | `await using` ensures deterministic disposal                      |
| Testing Complexity        | Provide base test fixtures for contributors                       |

## Alternatives Considered

### 1. Job recreation at the background job processor end
```csharp
internal sealed class MediaLibraryScanJobProcessorJob : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await foreach (IMediaLibraryScanJob job in _mediaScanQueue.Reader.ReadAllAsync(cancellationToken))
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            {
                IMediaLibraryScanJob scopedJob = (IMediaLibraryScanJob)scope.ServiceProvider.GetRequiredService(job.GetType());
                scopedJob.ScanId = originalJob.ScanId;
                scopedJob.UserId = originalJob.UserId;
                scopedJob.LibraryId = originalJob.LibraryId;
                await scopedJob.ExecuteAsync(Guid.NewGuid(), new { }, cancellationToken);
            }
        }
    }
}
```

**Rejected**: Destroys parent-child relationships between jobs. The domain model relies on pre-established directed acyclic graphs that cannot be rehydrated without violating encapsulation (See `IMediaTypeScanner.CreateScanJobsForLibrary()`).

### 2. Stateful handler pattern
```csharp
// requires splitting jobs into configuration/execution phases
public record JobState(ScanId, UserId, LibraryId);
public class JobHandler 
{
    public Task ExecuteAsync(JobState state) { /* ... */ }
}
```

**Rejected**: Would require complete architectural redesign of the library scanning aggregate, and would not easily scale to multiple jobs requiring different payload types.

### 3. Registration of `DbContextFactory` instead of `DbContext`
```csharp
services.AddDbContextFactory<LuminaDbContext>(options => 
{
    options.UseSqlite($"Data Source={Path.Combine(basePath, "Lumina.db")}");
}
```

**Rejected**: Would require a dependency on EF Core in the Application layer, and even when using an abstraction, would completely defeat the purpose of using the Repository + Unit of Work patterns used everywhere else in the Application layer.
