public static class Validator
{
    public static bool ValidateLogin(string login, out string error)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            error = "Логин не может быть пустым";
            return false;
        }
        if (login.Length < 3 || login.Length > 50)
        {
            error = "Логин должен быть от 3 до 50 символов";
            return false;
        }
        error = null;
        return true;
    }

    public static bool ValidatePassword(string password, out string error)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            error = "Пароль не может быть пустым";
            return false;
        }
        error = null;
        return true;
    }

    public static bool ValidatePasswordsMatch(string password, string confirmPassword, out string error)
    {
        if (password != confirmPassword)
        {
            error = "Пароли не совпадают";
            return false;
        }
        error = null;
        return true;
    }
}