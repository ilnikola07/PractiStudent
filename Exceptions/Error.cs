using System;
using System.Collections.Generic;

namespace Exceptions
{
    public class ValidationException : Exception // ошибки валидации класса
    {
        public List<string> Errors { get; }
        public ValidationException(string message) : base(message)
        {
            Errors = new List<string>();
        }
        public ValidationException(List<string> errors) : base("Ошибка валидации данных")
        {
            Errors = errors ?? new List<string>();
        }
        public void AddError(string error) => Errors.Add(error); // gозволяет наращивать список ошибок по мере проверки
    }

    public class DatabaseException : Exception // ошибки бд
    {
        public string TableName { get; } // имя таблицы
        public string Query { get; } // запрос, вызвавший ошибку

        public DatabaseException(string message, string tableName = null, string query = null)
            : base(message)
        {
            TableName = tableName;
            Query = query;
        }
    }

    public class AuthenticationException : Exception // ошибки входа
    {
        public AuthenticationException(string message) : base(message) { } 
    }

    public class BusinessException : Exception // ошибки в логике
    {
        public BusinessException(string message) : base(message) { }
    }
}