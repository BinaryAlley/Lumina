# Architecture Decision Record (ADR): 0003 - Pragmatic `System.IO` Usage for File System Discovery in Library Scan Job

## Status  
**Accepted** (2025-03-01)

## Context  
The media library scanning subsystem requires:  
1. **Performance at scale**: enumeration of files needs to run fast, because file discovery is the first job in the chain of media library scan jobs. If this is slow, the entire scan will be slow, no matter how optimized subsequent jobs are.
2. **Domain model limitations**:  
   - `FileSystemManagementAggregate` abstractions added over 16x latency overhead (65s vs 4s on tests ran on SSD).
   - Domain wrapper incurred unacceptable I/O amplification.
3. **Real-world constraints**:  
   - Users potentially having tens of millions of files and hundreds of terrabytes of data, in some cases.
   - Limited value from domain semantics during raw discovery.

## Decision  
Use `System.IO` directly in `FileSystemDiscoveryJob` with:  

```csharp
[MethodImpl(MethodImplOptions.AggressiveOptimization)]
private static List<FileInfo> GetFiles(...)
{
    // raw directory traversal using System.IO
    // (Domain aggregate bypassed)
}
```


## Consequences  

### Performance vs Purity Tradeoff  
| Metric               | Before (Domain) | After (System.IO) | Delta    |  
|----------------------|-----------------|-------------------|----------|  
| 100k File Enumeration| 65s             | 4s                | -94%     |

### Risks and Mitigations  
| Risk                          | Mitigation Strategy                                        |  
|-------------------------------|------------------------------------------------------------|  
| Hidden Platform Dependencies  | Regular integration tests on Linux/Windows                 |  
| Loss of Domain Abstraction    | Strictly limit usage to library scan discovery phase only  |  

## Alternatives Considered  

### 1. Optimized Domain Aggregate  

Domain entities like `File` or `Directory` could be modified to bypass invariants checks in their initialization, leading to a noticeable latency drop.

**Rejected**: Invariants checks are one of the most important domain aspects, and the performance gain was still insufficient for the target scale.

### 2. Hybrid Approach  

```csharp
if (fileCount > 10_000) 
    UseSystemIO();
else 
    UseDomainAggregate();
```

**Rejected**: Introduced unpredictable performance characteristics and potential for difference in behavior, hard to reproduce bugs that happen only on one branch, etc.

### 3. Platform-Specific Native Interop via P/Invoke

```csharp
// Windows implementation
[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

// Linux implementation
[DllImport("libc", EntryPoint = "opendir")]
static extern IntPtr OpenDir(string name);
```


**Rejected** due to:

| Risk                          | Impact Analysis                                                              |
|-------------------------------|------------------------------------------------------------------------------|
| Platform Fragility            | Required separate implementations for Windows/macOS/Linux (2x codebase size) |
| Path Encoding Bugs            | UTF-16/UTF-8 conversion errors in long paths (>32k chars)                    |
| Memory Leaks                  | Manual handle management required finalizers and safe handles                |
| Testing Complexity            | Physical VM requirements for platform validation                             |

### 5. Native C Library with Managed Interop

```c
// file_discovery.c
#ifdef _WIN32
#include <windows.h>
void win_enumerate(const char* path, void (callback)(const char)) 
{
    WIN32_FIND_DATA fd;
    HANDLE h = FindFirstFileEx(path, FindExInfoStandard, &fd, ...);
    // ... recursive implementation
}
#else
#include <dirent.h>
void lin_enumerate(const char* path, void (callback)(const char)) 
{
    DIR* dir = opendir(path);
    struct dirent* entry;
    // ... recursive implementation
}
#endif
```

**Rejected** due to:

| Risk                          | Maintenance Burden                                            |
|-------------------------------|---------------------------------------------------------------|
| Cross-Compilation Complexity  | Required CI pipeline support for Windows/macOS/Linux binaries |
| ABI Breakage                  | High risk of crashes during .NET runtime updates              |
| Debugging Difficulty          | Specialized tools (WinDbg/gdb) needed for crash analysis      |
