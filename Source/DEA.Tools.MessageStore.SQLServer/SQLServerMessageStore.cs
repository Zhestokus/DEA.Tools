using DEA.Core;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using System.Data;

namespace DEA.Tools.MessageStore.SQLServer
{
    public class SQLServerMessageStore : MessageStoreBase
    {
        private string _connectionString;
        private string _tableName;

        public SQLServerMessageStore(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;
        }

        public override void Begin()
        {
        }

        public override byte[] PopMessage(string eventName, Guid messageID)
        {
            var selectQuery = $"SELECT [ID], [Data] FROM {_tableName} WHERE [ID] = @ID";
            var deleteQuery = $"DELETE FROM {_tableName} WHERE [ID] =  @ID";

            using (var connection = new SqlConnection(_connectionString))
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new SqlCommand(string.Empty, connection, transaction))
                    {
                        command.CommandText = selectQuery;

                        command.Parameters.AddWithValue("@ID", messageID);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var index = reader.GetOrdinal("Data");
                                var data = (byte[])reader.GetValue(index);

                                return data;
                            }
                        }

                        command.CommandText = deleteQuery;
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }
            }

            return null;
        }
        public override async Task<byte[]> PopMessageAsync(string eventName, Guid messageID)
        {
            var selectQuery = $"SELECT [ID], [Data] FROM {_tableName} WHERE [ID] = @ID";
            var deleteQuery = $"DELETE FROM {_tableName} WHERE [ID] =  @ID";

            using (var connection = new SqlConnection(_connectionString))
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new SqlCommand(string.Empty, connection, transaction))
                    {
                        command.CommandText = selectQuery;

                        command.Parameters.AddWithValue("@ID", messageID);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                var index = reader.GetOrdinal("Data");
                                var data = (byte[])reader.GetValue(index);

                                return data;
                            }
                        }

                        command.CommandText = deleteQuery;
                        await command.ExecuteNonQueryAsync();

                        transaction.Commit();
                    }
                }
            }

            return null;
        }

        public override byte[] GetMessage(string eventName, Guid messageID)
        {
            var selectQuery = $"SELECT [ID], [Data] FROM {_tableName} WHERE [ID] = @ID";

            using (var connection = new SqlConnection(_connectionString))
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (var command = new SqlCommand(string.Empty, connection))
                {
                    command.CommandText = selectQuery;

                    command.Parameters.AddWithValue("@ID", messageID);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var index = reader.GetOrdinal("Data");
                            var data = (byte[])reader.GetValue(index);

                            return data;
                        }
                    }
                }
            }

            return null;
        }
        public override async Task<byte[]> GetMessageAsync(string eventName, Guid messageID)
        {
            var selectQuery = $"SELECT [ID], [Data] FROM {_tableName} WHERE [ID] = @ID";

            using (var connection = new SqlConnection(_connectionString))
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = new SqlCommand(string.Empty, connection))
                {
                    command.CommandText = selectQuery;

                    command.Parameters.AddWithValue("@ID", messageID);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var index = reader.GetOrdinal("Data");
                            var data = (byte[])reader.GetValue(index);

                            return data;
                        }
                    }
                }
            }

            return null;
        }

        public override void SaveMessage(string eventName, Guid messageID, byte[] messageData)
        {
            var insertQuery = $"INSERT INTO {_tableName}([ID], [Data]) VALUES (@ID, @Data)";

            using (var connection = new SqlConnection(_connectionString))
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new SqlCommand(string.Empty, connection, transaction))
                    {
                        command.CommandText = insertQuery;

                        command.Parameters.AddWithValue("@ID", messageID);
                        command.Parameters.AddWithValue("@Data", messageData);

                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }
            }
        }
        public override async Task SaveMessageAsync(string eventName, Guid messageID, byte[] messageData)
        {
            var insertQuery = $"INSERT INTO {_tableName}([ID], [Data]) VALUES (@ID, @Data)";

            using (var connection = new SqlConnection(_connectionString))
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new SqlCommand(string.Empty, connection, transaction))
                    {
                        command.CommandText = insertQuery;

                        command.Parameters.AddWithValue("@ID", messageID);
                        command.Parameters.AddWithValue("@Data", messageData);

                        await command.ExecuteNonQueryAsync();

                        transaction.Commit();
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }
    }
}
