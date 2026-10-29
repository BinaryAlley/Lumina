#region ========================================================================= USING =====================================================================================
using Dapper;
using System;
using System.Data;
#endregion

namespace Lumina.DataAccess.Common.Dapper;

/// <summary>
/// Maps <see cref="Guid"/> values to and from the TEXT representation used by the SQLite provider.
/// </summary>
/// <remarks>
/// The SQLite provider stores <see cref="Guid"/> columns as upper case TEXT. Dapper falls back to Convert.ChangeType when materializing
/// such columns into <see cref="Guid"/> properties. <see cref="Guid"/> has no string <see cref="TypeConverter"/>, so that conversion
/// throws an <see cref="InvalidCastException"/>; this handler performs the conversion explicitly instead, and writes the same upper case
/// representation so raw key comparisons and foreign keys match the rows created by Entity Framework.
/// </remarks>
internal sealed class SqliteGuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    /// <summary>
    /// Sets the parameter value to the upper case TEXT representation the SQLite provider uses for <see cref="Guid"/> columns.
    /// </summary>
    /// <param name="parameter">The database parameter to set.</param>
    /// <param name="value">The <see cref="Guid"/> value to store.</param>
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString().ToUpperInvariant();
    }

    /// <summary>
    /// Parses the raw TEXT value read from the storage medium into a <see cref="Guid"/>.
    /// </summary>
    /// <param name="value">The raw value read from the storage medium.</param>
    /// <returns>The parsed <see cref="Guid"/>.</returns>
    public override Guid Parse(object value)
    {
        return value is Guid guid ? guid : Guid.Parse((string)value);
    }
}
