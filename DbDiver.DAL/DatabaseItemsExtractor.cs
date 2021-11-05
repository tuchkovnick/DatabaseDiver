using DbDiver.Core;
using DbDiver.Core.Events;
using Microsoft.Data.Sqlite;
using Prism.Events;
using Prism;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbDiver.DAL
{
    public class DatabaseItemsExtractor : IDatabaseItemsExtractor
    {

        string _connectionString;
        SqliteConnection _connection;

        public DatabaseItemsExtractor(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new SqliteConnection(_connectionString);
            SQLitePCL.Batteries.Init();
            _connection.Open();
        }


        public IEnumerable<string> GetAllFoundValues(DbSearchParameter parameter)
        {
            List<string> result = new List<string>();
            var sqlExpression = $"SELECT * FROM {parameter.TableName} WHERE {parameter.ColumnName} LIKE '%{parameter.SearchItem}%'";
            SqliteCommand command = new SqliteCommand(sqlExpression, _connection);
            SqliteDataReader reader = command.ExecuteReader();            
            while (reader.Read())
            {
                StringBuilder rowPresentation = new StringBuilder();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    rowPresentation.Append($"[{reader.GetName(i)}]:{reader.GetString(i)} ");
                }
                result.Add(rowPresentation.ToString());
            }
            return result;               
        }
    }
}
