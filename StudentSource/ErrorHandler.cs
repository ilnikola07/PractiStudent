using System;
using System.Windows.Forms;
using Exceptions;

namespace StudentSource
{
    public static class ErrorHandler // классс обработчик сообщений и ошибок в приложении (вывод в MessageBox)
    {
        public static void Handle(Exception ex, string context = "") // обработка исключений, используется в блоках catch
        {
            Logger.LogError(ex.Message, ex, context);

            string message = ex.Message;
            string title = "Ошибка";
            MessageBoxIcon icon = MessageBoxIcon.Error;

            if (ex is ValidationException ve)
            {
                message = "Ошибка валидации:\n\n" + string.Join("\n", ve.Errors);
                title = "Ошибка валидации";
                icon = MessageBoxIcon.Warning;
            }
            else if (ex is DatabaseException de)
            {
                string tableName = string.IsNullOrEmpty(de.TableName) ? "" : $" ({de.TableName})";
                message = $"Ошибка базы данных{tableName}\n\n{de.Message}";
                title = "Ошибка БД";
            }
            else if (ex is AuthenticationException)
            {
                message = $"Ошибка авторизации:\n\n{ex.Message}";
                title = "Ошибка входа";
            }

            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }
        public static void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static bool AskQuestion(string message)
        {
            return MessageBox.Show(message, "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}