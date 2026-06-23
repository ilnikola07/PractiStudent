using System;
using System.Data;

namespace PractiStudent.Data
{
    public static class DataFormatter // класс форматирования даты в некоторых данных
    {
        /// <summary>
        ///  форматирует дату в таблице
        /// </summary>
        /// <param name="table"></param>
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

        /// <summary>
        /// форматирует одно значение для отображения в интерфейсе
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        /// <summary>
        /// является ли поле колонкой с датой
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        private static bool IsDateColumn(DataColumn col)
        {
            return col.DataType == typeof(DateTime) ||
                   col.ColumnName.Contains("Дата", StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// если вдруг есть время - вывести 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
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