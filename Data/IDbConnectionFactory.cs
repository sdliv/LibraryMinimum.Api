using System.Data;

namespace LibraryMiniumAPI.Data
{
    // Interface Factory for creating Database Connections.
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync();
    }
}
