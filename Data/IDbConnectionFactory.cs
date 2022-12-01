using System.Data;

namespace LibraryMinimumAPI.Data
{
    // Interface Factory for creating Database Connections.
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync();
    }
}
