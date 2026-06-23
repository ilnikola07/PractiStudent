using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Exceptions;

namespace PractiStudent.Data
{
    public class DatabaseHelper  // класс для работы со слоем данных
    {
        private string _connectionString;
        public string DatabaseFileName { get; private set; }
        public DatabaseHelper()
        {
            _connectionString = null;
            DatabaseFileName = null;
        }

        /// <summary>
        /// устанавливает подключение к БД
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
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
        /// <summary>
        /// существует ли таблица в БД
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
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
        /// <summary>
        /// есть ли подключение
        /// </summary>
        /// <exception cref="DatabaseException"></exception>
        private void EnsureConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new DatabaseException("Подключение к БД не установлено");
        }

        //public string GetConnectionString() => _connectionString;

        /// <summary>
        /// получает список всех таблиц
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// получает список всех полей в таблице
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// получает все данные в таблице
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable GetAllData(string tableName)
        {
            EnsureConnection();
            return ExecuteSelect($"SELECT * FROM [{tableName}]");
        }

        /// <summary>
        /// поиск записи по частичному совпадению
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public DataTable SearchData(string tableName, string columnName, string searchText)
        {
            EnsureConnection();
            string query = $"SELECT * FROM [{tableName}] WHERE [{columnName}] LIKE ?";
            OleDbParameter[] parameters = { new OleDbParameter("?", $"%{searchText}%") };
            return ExecuteSelect(query, parameters);
        }

        /// <summary>
        /// фильтрация данных (поиск по точному совпадению)
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        public DataTable FilterData(string tableName, string columnName, string filterValue)
        {
            EnsureConnection();
            string query = $"SELECT * FROM [{tableName}] WHERE [{columnName}] = ?";
            OleDbParameter[] parameters = { new OleDbParameter("?", filterValue) };
            return ExecuteSelect(query, parameters);
        }

        /// <summary>
        /// сортировка данных
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public DataTable SortData(string tableName, string columnName, bool ascending)
        {
            EnsureConnection();
            string order = ascending ? "ASC" : "DESC";
            string query = $"SELECT * FROM [{tableName}] ORDER BY [{columnName}] {order}";
            return ExecuteSelect(query);
        }

        /// <summary>
        /// добавить новую запись
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
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

        /// <summary>
        /// обновить запись в указанной таблице
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="values"></param>
        /// <param name="keyColumn"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
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

        /// <summary>
        /// удалить запись по значению первичного ключа
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="keyColumn"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public int DeleteRecord(string tableName, string keyColumn, object keyValue)
        {
            EnsureConnection();
            string query = $"DELETE FROM [{tableName}] WHERE [{keyColumn}] = ?";
            OleDbParameter[] parameters = { new OleDbParameter("?", keyValue) };
            return ExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// получает список уникальных значений в колонке таблицы
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// получает список уникальных колонок
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// выполняет SQL запрос, не возвращающий данных 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
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

        /// <summary>
        /// выполняет SQL запрос и возвращает одно значение (первый столбец первой строки)
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
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

        /// <summary>
        ///  выполняет SQL запрос SELECT и возвращает результат в виде DataTable
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Проверка существования логина в базе данных
        /// </summary>
        public bool IsLoginExists(string login)
        {
            string query = "SELECT COUNT(*) FROM [Пользователи] WHERE [Логин] = ?";
            OleDbParameter[] parameters = { new OleDbParameter("?", login) };

            object result = ExecuteScalar(query, parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Получение роли пользователя по логину и хэшу пароля
        /// </summary>
        public string GetUserRole(string login, string passwordHash)
        {
            string query = "SELECT [Роль] FROM [Пользователи] WHERE [Логин] = ? AND [Хэш_пароля] = ?";
            OleDbParameter[] parameters = {
        new OleDbParameter("?", login),
        new OleDbParameter("?", passwordHash)
    };

            object result = ExecuteScalar(query, parameters);
            return result?.ToString();
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        public bool InsertUser(string login, string passwordHash, string role)
        {
            string query = "INSERT INTO [Пользователи] ([Логин], [Хэш_пароля], [Роль]) VALUES (?, ?, ?)";
            OleDbParameter[] parameters = {
        new OleDbParameter("?", login),
        new OleDbParameter("?", passwordHash),
        new OleDbParameter("?", role)
    };

            int rowsAffected = ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Получение количества администраторов в системе
        /// </summary>
        public int GetAdminCount()
        {
            string query = "SELECT COUNT(*) FROM [Пользователи] WHERE [Роль] = ?";
            OleDbParameter[] parameters = { new OleDbParameter("?", "Администратор") };

            object result = ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// удаление пользователя по логину
        /// </summary>
        public bool DeleteUser(string login)
        {
            string query = "DELETE FROM [Пользователи] WHERE [Логин] = ?";
            OleDbParameter[] parameters = { new OleDbParameter("?", login) };

            int rowsAffected = ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// удаление каскадом
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="keyColumn"></param>
        /// <param name="keyValue"></param>
        /// <param name="relatedTables"></param>
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