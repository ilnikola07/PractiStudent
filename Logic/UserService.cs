using System;
using PractiStudent.Data;

namespace Logic
{
    public class UserService // класс, отвечающий за работу с пользователями
    {
        private DatabaseHelper _dbHelper;
        private string _databaseFileName;

        public UserService() 
        {
            _dbHelper = new DatabaseHelper();
        }

        /// <summary>
        /// подключение к базе данных
        /// </summary>
        public bool ConnectToDatabase(string filePath)
        {
            bool result = _dbHelper.SetDatabaseConnection(filePath);
            if (result)
            {
                _databaseFileName = System.IO.Path.GetFileName(filePath);
            }
            return result;
        }

        /// <summary>
        /// получение имени файла базы данных
        /// </summary>
        public string GetDatabaseFileName()
        {
            return _databaseFileName;
        }

        /// <summary>
        /// валидация учётных данных пользователя
        /// </summary>
        public string ValidateUser(string login, string password, string expectedRole)
        {
            string hashedPassword = SecurityHelper.ComputeSha256Hash(password);
            string role = _dbHelper.GetUserRole(login, hashedPassword);

            if (role != null && role == expectedRole)
            {
                return expectedRole;
            }

            return null;
        }

        /// <summary>
        /// регистрация нового гостя
        /// </summary>
        public bool RegisterGuest(string login, string password)
        {
            if (_dbHelper.IsLoginExists(login))
            {
                throw new Exception("Пользователь с таким логином уже существует!");
            }

            string passwordHash = SecurityHelper.ComputeSha256Hash(password);
            return _dbHelper.InsertUser(login, passwordHash, "Гость");
        }

        /// <summary>
        /// проверка возможности удаления пользователя
        /// </summary>
        public bool CanDeleteUser(string loginToDelete, string currentUserRole)
        {
            if (currentUserRole == "Администратор")
            {
                int adminCount = _dbHelper.GetAdminCount();
                if (adminCount <= 1)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// удаление пользователя
        /// </summary>
        public bool DeleteUser(string login)
        {
            return _dbHelper.DeleteUser(login);
        }
    }
}