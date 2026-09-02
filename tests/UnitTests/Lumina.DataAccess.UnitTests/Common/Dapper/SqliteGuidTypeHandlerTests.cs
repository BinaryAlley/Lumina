#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Common.Dapper;
using NSubstitute;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Dapper;

/// <summary>
/// Contains unit tests for the <see cref="SqliteGuidTypeHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SqliteGuidTypeHandlerTests
{
    private readonly SqliteGuidTypeHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteGuidTypeHandlerTests"/> class.
    /// </summary>
    public SqliteGuidTypeHandlerTests()
    {
        _sut = new SqliteGuidTypeHandler();
    }

    [Fact]
    public void SetValue_WhenCalled_ShouldStoreTheGuidAsItsTextRepresentation()
    {
        // Arrange
        IDbDataParameter parameter = Substitute.For<IDbDataParameter>();
        Guid guid = Guid.NewGuid();

        // Act
        _sut.SetValue(parameter, guid);

        // Assert
        Assert.Equal(guid.ToString(), (string?)parameter.Value);
    }

    [Fact]
    public void Parse_WhenValueIsGuid_ShouldReturnTheSameGuid()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        object value = guid;

        // Act
        Guid parsed = _sut.Parse(value);

        // Assert
        Assert.Equal(guid, parsed);
    }

    [Fact]
    public void Parse_WhenValueIsGuidText_ShouldReturnTheParsedGuid()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        object value = guid.ToString();

        // Act
        Guid parsed = _sut.Parse(value);

        // Assert
        Assert.Equal(guid, parsed);
    }

    [Fact]
    public void Parse_WhenValueIsNotValidGuidText_ShouldThrowFormatException()
    {
        // Arrange
        object value = "not-a-guid";

        // Act
        Action act = () => _sut.Parse(value);

        // Assert
        Assert.Throws<FormatException>(act);
    }
}
