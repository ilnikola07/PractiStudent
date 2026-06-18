using System;
using System.Collections.Generic;

namespace Exceptions
{
    // Исключение валидации данных
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string>();
        }

        public ValidationException(List<string> errors) : base("Ошибка валидации данных")
        {
            Errors = errors;
        }

        public void AddError(string error) => Errors.Add(error);
    }

    // Исключение работы с БД
    public class DatabaseException : Exception
    {
        public string TableName { get; }
        public string Query { get; }

        public DatabaseException(string message, string tableName = null, string query = null)
            : base(message)
        {
            TableName = tableName;
            Query = query;
        }
    }

    // Исключение авторизации
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
    }

    // Исключение бизнес-логики
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }
}