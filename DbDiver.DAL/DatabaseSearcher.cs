using DbDiver.Core;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace DbDiver.DAL
{
    public class DatabaseSearcher : IDatabaseSearcher
    {
        string _connectionString;

        public DatabaseSearcher(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool HasParameter(DbSearchParameter parameter)
        {            
            using (var connection = new SqliteConnection(_connectionString))
            {
                SQLitePCL.Batteries.Init();
                connection.Open();
                
               var sqlExpression = $"SELECT 1 FROM {parameter.TableName} WHERE {parameter.ColumnName} LIKE '%{parameter.SearchItem}%'";
               SqliteCommand command = new SqliteCommand(sqlExpression, connection);
               SqliteDataReader reader = command.ExecuteReader();

               return reader.HasRows;
                
            }
        }
    }
}
