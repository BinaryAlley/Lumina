#region ========================================================================= USING =====================================================================================
using Dapper;
#endregion

namespace Lumina.DataAccess.Common.Dapper;

/// <summary>
/// Registers the Dapper type handlers used by the DataAccess layer.
/// </summary>
internal static class DapperTypeHandlers
{
    /// <summary>
    /// Registers all Dapper type handlers used by the DataAccess layer, and must run before any Dapper query executes.
    /// </summary>
    internal static void Register()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
    }
}
