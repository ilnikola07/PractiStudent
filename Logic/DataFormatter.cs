using System;
using System.Data;

namespace Logic
{
    public static class DataFormatter
    {
        public static void FormatDatesInTable(DataTable table)
        {
            foreach (DataColumn col in table.Columns)
            {
                if (col.DataType == typeof(DateTime) ||
                    col.ColumnName.Contains("Дата") ||
                    col.ColumnName.Contains("дата"))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (row[col] != DBNull.Value)
                        {
                            DateTime dt = Convert.ToDateTime(row[col]);
                            if (dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0)
                            {
                                row[col] = dt.ToString("dd.MM.yyyy");
                            }
                        }
                    }
                }
            }
        }

        public static string FormatValueForDisplay(object value)
        {
            if (value == null || value == DBNull.Value)
                return "";

            string strValue = value.ToString();

            if (DateTime.TryParse(strValue, out DateTime dt))
            {
                if (dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0)
                {
                    return dt.ToString("dd.MM.yyyy");
                }
                return dt.ToString("dd.MM.yyyy HH:mm");
            }

            return strValue;
        }
    }
}