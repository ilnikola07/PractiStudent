using System;
using System.Data;
using System.Data.OleDb;

namespace PractiStudent.Data
{
    public class DatabaseHelper
    {
        // База данных должна лежать в папке с исполняемым файлом (bin\Debug)
        private readonly string _connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=|DataDirectory|\Abiturient.accdb;";

        // 1. Метод для выполнения запросов изменения (INSERT, UPDATE, DELETE)
        public int ExecuteNonQuery(string query, OleDbParameter[] parameters = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // 2. Метод для получения одного значения (например, COUNT или роли)
        public object ExecuteScalar(string query, OleDbParameter[] parameters = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }

        // 3. Метод для получения таблицы данных (пригодится для вывода в DataGridView)
        public DataTable ExecuteSelect(string query, OleDbParameter[] parameters = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }
    }
}
