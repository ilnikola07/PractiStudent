using System.Data.OleDb;
using PractiStudent.Data;
using Exceptions;  

namespace Logic
{
    public class UserService // Класс отвечающий за регистрацию пользователей и вход
    {
        private readonly DatabaseHelper _dbHelper;
        private bool _isConnected = false;

        public UserService()
        {
            _dbHelper = new DatabaseHelper();
        }
                
        public bool ConnectToDatabase(string filePath) // Метод для подключения к выбранной бд
        {
            _isConnected = _dbHelper.SetDatabaseConnection(filePath);

            if (_isConnected)
            {
                EnsureAdminCreated();
            }

            return _isConnected;
        }

        public bool CanDeleteUser(string loginToDelete, string currentLoggedInUser)
        {             
            string query = "SELECT COUNT(*) FROM [Пользователи] WHERE [Роль] = ? AND [Логин] != ?";
            OleDbParameter[] parameters = new OleDbParameter[] // Нельзя удалить последнего админа
            {
        new OleDbParameter("?", "Администратор"),
        new OleDbParameter("?", loginToDelete)
            };

            try
            {
                int adminCount = Convert.ToInt32(_dbHelper.ExecuteScalar(query, parameters));
                return adminCount > 0; // Можно удалять только если есть другие админы
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки админов: {ex.Message}");
                return false; // В случае ошибки запрещаем удаление
            }
        }
        public string GetDatabaseFileName()
        {
            return _dbHelper.DatabaseFileName;
        }
        public void EnsureAdminCreated()
        {
            try
            {
                string adminLogin = "admin";
                if (!IsLoginExists(adminLogin))
                {
                    string passwordHash = SecurityHelper.ComputeSha256Hash("admin123");
                    string query = "INSERT INTO Пользователи (Логин, Хэш_Пароля, Роль) VALUES (?, ?, ?)";

                    OleDbParameter[] parameters = new OleDbParameter[]
                    {
                        new OleDbParameter("?", adminLogin),
                        new OleDbParameter("?", passwordHash),
                        new OleDbParameter("?", "Администратор")
                    };

                    _dbHelper.ExecuteNonQuery(query, parameters);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания админа: {ex.Message}");
            }
        }
        public bool RegisterGuest(string login, string password)
        {
            if (IsLoginExists(login))
            {
                throw new Exception("Пользователь с таким логином уже существует!");
            }

            string passwordHash = SecurityHelper.ComputeSha256Hash(password);
            string query = "INSERT INTO Пользователи (Логин, Хэш_Пароля, Роль) VALUES (@login, @hash, @role)";

            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@login", login),
                new OleDbParameter("@hash", passwordHash),
                new OleDbParameter("@role", "Гость")
            };

            int rows = _dbHelper.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        public string ValidateUser(string login, string password, string expectedRole)
        {
            string passwordHash = SecurityHelper.ComputeSha256Hash(password);
            string query = "SELECT Роль FROM Пользователи WHERE Логин = ? AND Хэш_Пароля = ? AND Роль = ?";

            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("?", login),
                new OleDbParameter("?", passwordHash),
                new OleDbParameter("?", expectedRole)
            };
            object result = _dbHelper.ExecuteScalar(query, parameters);
            return result?.ToString();
        }

        private bool IsLoginExists(string login)
        {
            string query = "SELECT COUNT(*) FROM Пользователи WHERE Логин = ?";
            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("?", login)
            };

            int count = Convert.ToInt32(_dbHelper.ExecuteScalar(query, parameters));
            return count > 0;
        }
    }
}