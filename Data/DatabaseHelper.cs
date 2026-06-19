using System;
using System.Collections.Generic;
using System.Data;
using Exceptions;  
using System.Data.OleDb;

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
            try
            {
                if (!System.IO.File.Exists(filePath))
                    return false;

                _connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";
                DatabaseFileName = System.IO.Path.GetFileName(filePath);

                using (OleDbConnection conn = new OleDbConnection(_connectionString))
                {
                    conn.Open();
                    var schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                        new object[] { null, null, null, "TABLE" });

                    bool tableExists = false;
                    if (schema != null)
                    {
                        foreach (DataRow row in schema.Rows)
                        {
                            if (row["TABLE_NAME"].ToString() == "Пользователи")
                            {
                                tableExists = true;
                                break;
                            }
                        }
                    }
                    return tableExists;
                }
            }
            catch
            {
                return false;
            }
        }

        public string GetConnectionString() => _connectionString;

        // Получить список всех таблиц в БД
        public List<string> GetTableNames()
        {
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
                        // Исключаем системные таблицы
                        if (!tableName.StartsWith("MSys"))
                        {
                            tables.Add(tableName);
                        }
                    }
                }
            }
            return tables;
        }

        // Получить структуру таблицы (имена столбцов)
        public List<string> GetColumnNames(string tableName)
        {
            List<string> columns = new List<string>();
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                conn.Open();
                var schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                    new object[] { null, null, tableName, null });

                if (schema != null)
                {
                    foreach (DataRow row in schema.Rows)
                    {
                        columns.Add(row["COLUMN_NAME"].ToString());
                    }
                }
            }
            return columns;
        }

        // Получить все данные из таблицы
        public DataTable GetAllData(string tableName)
        {
            string query = $"SELECT * FROM [{tableName}]";
            return ExecuteSelect(query);
        }

        // Поиск по таблице
        public DataTable SearchData(string tableName, string columnName, string searchText)
        {
            string query = $"SELECT * FROM [{tableName}] WHERE [{columnName}] LIKE ?";
            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("?", $"%{searchText}%")
            };
            return ExecuteSelect(query, parameters);
        }

        // Фильтрация по столбцу и значению (ТОЧНОЕ совпадение)
        public DataTable FilterData(string tableName, string columnName, string filterValue)
        {
            string query = $"SELECT * FROM [{tableName}] WHERE [{columnName}] = ?";
            OleDbParameter[] parameters = new OleDbParameter[]
            {
        new OleDbParameter("?", filterValue)
            };
            return ExecuteSelect(query, parameters);
        }

        // Сортировка данных
        public DataTable SortData(string tableName, string columnName, bool ascending)
        {
            string order = ascending ? "ASC" : "DESC";
            string query = $"SELECT * FROM [{tableName}] ORDER BY [{columnName}] {order}";
            return ExecuteSelect(query);
        }

        // Добавление записи
        public int InsertRecord(string tableName, Dictionary<string, object> values)
        {
            string columns = string.Join(", ", values.Keys.Select(k => $"[{k}]"));
            string placeholders = string.Join(", ", values.Keys.Select(k => "?"));
            string query = $"INSERT INTO [{tableName}] ({columns}) VALUES ({placeholders})";

            OleDbParameter[] parameters = values.Values.Select(v =>
                new OleDbParameter("?", v ?? DBNull.Value)).ToArray();

            return ExecuteNonQuery(query, parameters);
        }

        // Обновление записи
        public int UpdateRecord(string tableName, Dictionary<string, object> values, string keyColumn, object keyValue)
        {
            string setClause = string.Join(", ", values.Keys.Select(k => $"[{k}] = ?"));
            string query = $"UPDATE [{tableName}] SET {setClause} WHERE [{keyColumn}] = ?";

            List<OleDbParameter> parameters = new List<OleDbParameter>();
            foreach (var val in values.Values)
            {
                parameters.Add(new OleDbParameter("?", val ?? DBNull.Value));
            }
            parameters.Add(new OleDbParameter("?", keyValue));

            return ExecuteNonQuery(query, parameters.ToArray());
        }

        // Удаление записи
        public int DeleteRecord(string tableName, string keyColumn, object keyValue)
        {
            string query = $"DELETE FROM [{tableName}] WHERE [{keyColumn}] = ?";
            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("?", keyValue)
            };
            return ExecuteNonQuery(query, parameters);
        }

        // Получить уникальные значения столбца (для фильтра)
        public List<string> GetDistinctValues(string tableName, string columnName)
        {
            List<string> values = new List<string>();
            string query = $"SELECT DISTINCT [{columnName}] FROM [{tableName}]";
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    conn.Open();
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            values.Add(reader[0].ToString());
                        }
                    }
                }
            }
            return values;
        }

        public int ExecuteNonQuery(string query, OleDbParameter[] parameters = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(string query, OleDbParameter[] parameters = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }

        public DataTable ExecuteSelect(string query, OleDbParameter[] parameters = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
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
        }

        public List<string> GetUniqueColumns(string tableName)
        {
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
                        bool isUnique = Convert.ToBoolean(row["UNIQUE"]);
                        if (isUnique)
                        {
                            string columnName = row["COLUMN_NAME"].ToString();
                            if (!uniqueColumns.Contains(columnName))
                            {
                                uniqueColumns.Add(columnName);
                            }
                        }
                    }
                }
            }

            return uniqueColumns;
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
                        // Сначала удаляем связанные записи
                        foreach (var related in relatedTables)
                        {
                            string relatedTable = related.Key;
                            string foreignKey = related.Value;
                            string query = $"DELETE FROM [{relatedTable}] WHERE [{foreignKey}] = ?";
                            using (OleDbCommand cmd = new OleDbCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@p1", keyValue);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Затем основную запись
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