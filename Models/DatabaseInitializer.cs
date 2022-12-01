﻿using Dapper;
using LibraryMinimumAPI.Data;

namespace LibraryMinimumAPI.Models
{
    public class DatabaseInitializer
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DatabaseInitializer(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InitializeAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(
                @"CREATE TABLE IF NOT EXISTS BOOKS(
                Isbn TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Author TEXT NOT NULL,
                ShortDescription TEXT NOT NULL,
                PageCount INTEGER,
                ReleaseDate TEXT NOT NULL)"
            );
        }
    }
}
