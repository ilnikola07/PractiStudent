using System;
using System.Windows.Forms;
using Exceptions;  

namespace StudentSource
{
    public static class ErrorHandler
    {
        public static void Handle(Exception ex, string context = "")
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
                message = $"Ошибка базы данных{(!string.IsNullOrEmpty(de.TableName) ? $" ({de.TableName})" : "")}\n\n{de.Message}";
                title = "Ошибка БД";
            }
            else if (ex is AuthenticationException ae)
            {
                message = $"Ошибка авторизации:\n\n{ae.Message}";
                title = "Ошибка входа";
            }

            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        public static void ShowInfo(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowWarning(string message, string title = "Предупреждение")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool AskQuestion(string message, string title = "Подтверждение")
        {
            return MessageBox.Show(message, title,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}