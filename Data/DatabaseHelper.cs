using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Exceptions;

namespace PractiStudent.Data
{
    public class DatabaseHelper
    {
        private string _connectionString;
        public string DatabaseFileName { get; private set; }
        public DatabaseHelper()
        {
            _connectionString = null;
            DatabaseFileName = null;
        }
        public bool SetDatabaseConnection(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Logger.LogError("Путь к файлу пустой", context: "SetDatabaseConnection");
                return false;
            }

            if (!File.Exists(filePath))
            {
                Logger.LogError($"Файл не найден: {filePath}", context: "SetDatabaseConnection");
                return false;
            }

            try
            {
                _connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";
                DatabaseFileName = Path.GetFileName(filePath);

                using (OleDbConnection conn = new OleDbConnection(_connectionString))
                {
                    conn.Open();
                    return CheckTableExists(conn, "Пользователи");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка подключения к БД: {ex.Message}", ex, "SetDatabaseConnection");
                return false;
            }
        }
        private bool CheckTableExists(OleDbConnection conn, string tableName)
        {
            var schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "TABLE" });

            if (schema != null)
            {
                foreach (DataRow row in schema.Rows)
                {
                    if (row["TABLE_NAME"].ToString() == tableName)
                        return true;
                }
            }
            return false;
        }
        private void EnsureConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new DatabaseException("Подключение к БД не установлено");
        }
        public string GetConnectionString() => _connectionString;

        public List<string> GetTableNames()
        {
            EnsureConnection();
            List<string> tables = new List<string>();

            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                conn.Open();
                var schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                    new object[] { null, null, null, "TABLE" });

                if (schema != null)
                {
                    foreach (DataRow row in schema.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        if (!tableName.StartsWith("MSys"))
                            tables.Add(tableName);
                    }
                }
            }
            return tables;
        }
        public List<string> GetColumnNames(string tableName)
        {
            EnsureConnection();
            List<string> columns = new List<string>();

            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                conn.Open();
                var schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                    new object[] { null, null, tableName, null });

                if (schema != null)
                {
                    foreach (DataRow row in schema.Rows)
                        columns.Add(row["COLUMN_NAME"].ToString());
                }
            }
            return columns;
        }
        public DataTable GetAllData(string tableName)
        {
            EnsureConnection();
            return ExecuteSelect($"SELECT * FROM [{tableName}]");
        }
        public DataTable SearchData(string tableName, string columnName, string searchText)
        {
            EnsureConnection();
            string query = $"SELECT * FROM [{tableName}] WHERE [{columnName}] LIKE ?";
            OleDbParameter[] parameters = { new OleDbParameter("?", $"%{searchText}%") };
            return ExecuteSelect(query, parameters);
        }
        public DataTable FilterData(string tableName, string columnName, string filterValue)
        {
            EnsureConnection();
            string query = $"SELECT * FROM [{tableName}] WHERE [{columnName}] = ?";
            OleDbParameter[] parameters = { new OleDbParameter("?", filterValue) };
            return ExecuteSelect(query, parameters);
        }
        public DataTable SortData(string tableName, string columnName, bool ascending)
        {
            EnsureConnection();
            string order = ascending ? "ASC" : "DESC";
            string query = $"SELECT * FROM [{tableName}] ORDER BY [{columnName}] {order}";
            return ExecuteSelect(query);
        }
        public int InsertRecord(string tableName, Dictionary<string, object> values)
        {
            EnsureConnection();
            string columns = string.Join(", ", values.Keys.Select(k => $"[{k}]"));
            string placeholders = string.Join(", ", values.Keys.Select(k => "?"));
            string query = $"INSERT INTO [{tableName}] ({columns}) VALUES ({placeholders})";

            OleDbParameter[] parameters = values.Values
                .Select(v => new OleDbParameter("?", v ?? DBNull.Value))
                .ToArray();

            return ExecuteNonQuery(query, parameters);
        }
        public int UpdateRecord(string tableName, Dictionary<string, object> values,
            string keyColumn, object keyValue)
        {
            EnsureConnection();
            string setClause = string.Join(", ", values.Keys.Select(k => $"[{k}] = ?"));
            string query = $"UPDATE [{tableName}] SET {setClause} WHERE [{keyColumn}] = ?";

            List<OleDbParameter> parameters = values.Values
                .Select(v => new OleDbParameter("?", v ?? DBNull.Value))
                .ToList();
            parameters.Add(new OleDbParameter("?", keyValue));

            return ExecuteNonQuery(query, parameters.ToArray());
        }
        public int DeleteRecord(string tableName, string keyColumn, object keyValue)
        {
            EnsureConnection();
            string query = $"DELETE FROM [{tableName}] WHERE [{keyColumn}] = ?";
            OleDbParameter[] parameters = { new OleDbParameter("?", keyValue) };
            return ExecuteNonQuery(query, parameters);
        }
        public List<string> GetDistinctValues(string tableName, string columnName)
        {
            EnsureConnection();
            List<string> values = new List<string>();
            string query = $"SELECT DISTINCT [{columnName}] FROM [{tableName}]";

            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            using (OleDbCommand cmd = new OleDbCommand(query, conn))
            {
                conn.Open();
                using (OleDbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        values.Add(reader[0].ToString());
                }
            }
            return values;
        }
        public List<string> GetUniqueColumns(string tableName)
        {
            EnsureConnection();
            List<string> uniqueColumns = new List<string>();

            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                conn.Open();
                var schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes,
                    new object[] { null, null, tableName, null });

                if (schema != null)
                {
                    foreach (DataRow row in schema.Rows)
                    {
                        if (Convert.ToBoolean(row["UNIQUE"]))
                        {
                            string columnName = row["COLUMN_NAME"].ToString();
                            if (!uniqueColumns.Contains(columnName))
                                uniqueColumns.Add(columnName);
                        }
                    }
                }
            }
            return uniqueColumns;
        }
        public int ExecuteNonQuery(string query, OleDbParameter[] parameters = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            using (OleDbCommand cmd = new OleDbCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
        public object ExecuteScalar(string query, OleDbParameter[] parameters = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            using (OleDbCommand cmd = new OleDbCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
        public DataTable ExecuteSelect(string query, OleDbParameter[] parameters = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            using (OleDbCommand cmd = new OleDbCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
        public void DeleteWithCascade(string tableName, string keyColumn, object keyValue,
            Dictionary<string, string> relatedTables)
        {
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                conn.Open();
                using (OleDbTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var related in relatedTables)
                        {
                            string query = $"DELETE FROM [{related.Key}] WHERE [{related.Value}] = ?";
                            using (OleDbCommand cmd = new OleDbCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@p1", keyValue);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        string mainQuery = $"DELETE FROM [{tableName}] WHERE [{keyColumn}] = ?";
                        using (OleDbCommand cmd = new OleDbCommand(mainQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@p1", keyValue);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}