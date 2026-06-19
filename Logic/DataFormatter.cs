using System;
using System.Data;

namespace Logic
{
    public static class DataFormatter
    {
        public static void FormatDatesInTable(DataTable table)
        {
            if (table == null) return;

            foreach (DataColumn col in table.Columns)
            {
                if (IsDateColumn(col))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (row[col] != DBNull.Value)
                        {
                            DateTime dt = Convert.ToDateTime(row[col]);
                            row[col] = FormatDate(dt);
                        }
                    }
                }
            }
        }
        public static string FormatValueForDisplay(object value)
        {
            if (value == null || value == DBNull.Value)
                return "";

            if (DateTime.TryParse(value.ToString(), out DateTime dt))
            {
                return FormatDate(dt);
            }

            return value.ToString();
        }
        private static bool IsDateColumn(DataColumn col)
        {
            return col.DataType == typeof(DateTime) ||
                   col.ColumnName.Contains("Дата", StringComparison.OrdinalIgnoreCase);
        }
        private static string FormatDate(DateTime dt)
        {
            if (dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0)
            {
                return dt.ToString("dd.MM.yyyy");
            }
            return dt.ToString("dd.MM.yyyy HH:mm");
        }
    }
}