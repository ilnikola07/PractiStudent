using System;
using System.Data;
using System.IO;
using System.Text;

namespace Logic
{
    public static class ReportGenerator
    {
        private const int LineWidth = 80;
        private const int ColumnWidth = 20;

        public static void CreateWordReport(DataTable data, string tableName,
            string userRole, string filePath)
        {
            if (data == null) 
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='utf-8'></head><body>");
            sb.AppendLine($"<h1>Отчёт по таблице: {tableName}</h1>");
            sb.AppendLine($"<p>Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}</p>");
            sb.AppendLine($"<p>Пользователь: {userRole}</p>");
            sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0'>");

            sb.AppendLine("<tr>");
            foreach (DataColumn col in data.Columns)
            {
                sb.AppendLine($"<th><b>{col.ColumnName}</b></th>");
            }
            sb.AppendLine("</tr>");

            foreach (DataRow row in data.Rows)
            {
                sb.AppendLine("<tr>");
                foreach (DataColumn col in data.Columns)
                {
                    sb.AppendLine($"<td>{row[col]}</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine($"<p>Всего записей: {data.Rows.Count}</p>");
            sb.AppendLine("</body></html>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        public static void CreateTextReport(DataTable data, string tableName,
            string userRole, string filePath)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"ОТЧЁТ ПО ТАБЛИЦЕ: {tableName}");
            sb.AppendLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine($"Пользователь: {userRole}");
            sb.AppendLine(new string('-', LineWidth));

            string header = "";
            foreach (DataColumn col in data.Columns)
            {
                header += col.ColumnName.PadRight(ColumnWidth);
            }
            sb.AppendLine(header);
            sb.AppendLine(new string('-', LineWidth));

            foreach (DataRow row in data.Rows)
            {
                string line = "";
                foreach (DataColumn col in data.Columns)
                {
                    line += row[col].ToString().PadRight(ColumnWidth);
                }
                sb.AppendLine(line);
            }

            sb.AppendLine(new string('-', LineWidth));
            sb.AppendLine($"Всего записей: {data.Rows.Count}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}