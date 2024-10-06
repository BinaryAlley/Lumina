#region ========================================================================= USING =====================================================================================
using Bogus;
using ErrorOr;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.ValueObjects.Fixtures;

/// <summary>
/// Fixture class for the <see cref="FileSystemPathId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemPathIdFixture
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly Faker _faker;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemPathIdFixture"/> class.
    /// </summary>
    public FileSystemPathIdFixture()
    {
        _faker = new Faker();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a random valid <see cref="FileSystemPathId"/>.
    /// </summary>
    /// <param name="path">The path of the file system path element.</param>
    /// <returns>The created <see cref="FileSystemPathId"/>.</returns>
    public FileSystemPathId CreateFileSystemPathId(string? path = null)
    {
        path ??= _faker.System.FilePath();

        ErrorOr<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);

        if (fileSystemPathIdResult.IsError)
            throw new InvalidOperationException("Failed to create FileSystemPathId: " + string.Join(", ", fileSystemPathIdResult.Errors));
        return fileSystemPathIdResult.Value;
    }
    #endregion
}