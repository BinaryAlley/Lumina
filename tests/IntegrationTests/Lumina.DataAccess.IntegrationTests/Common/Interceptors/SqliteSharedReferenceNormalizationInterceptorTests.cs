#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.UoW;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Common.Interceptors;

/// <summary>
/// Contains integration tests for the <see cref="SqliteSharedReferenceNormalizationInterceptor"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SqliteSharedReferenceNormalizationInterceptorTests
{
    [Theory]
    [InlineData("  Horror  ", "horror")] // ASCII letters
    [InlineData("  ÉLECTRO  ", "électro")] // non-ASCII Latin letters
    [InlineData("ЖАНР", "жанр")] // non-Latin letters
    public async Task ConnectionOpenedAsync_WhenTheConnectionIsOpened_ShouldRegisterTheInvariantNormalizationFunction(string name, string expectedName)
    {
        // Arrange
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);

        // Act
        await context.Database.OpenConnectionAsync();
        SqliteConnection connection = (SqliteConnection)context.Database.GetDbConnection();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT {SqliteSharedReferenceNormalizationInterceptor.NORMALIZE_FUNCTION_NAME}($name);";
        command.Parameters.AddWithValue("$name", name);
        object? result = await command.ExecuteScalarAsync();

        // Assert
        Assert.Equal(expectedName, result);
    }
}
