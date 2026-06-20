namespace Logic
{
    public static class Validator
    {
        public static List<string> ValidateCredentials(string login, string password, string confirmPassword = null, bool isRegistration = false)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(login))
            {
                errors.Add("Логин не может быть пустым");
            }
            else if (login.Length < 3 || login.Length > 50)
            {
                errors.Add("Логин должен быть от 3 до 50 символов");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Пароль не может быть пустым");
            }

            if (isRegistration)
            {
                if (string.IsNullOrWhiteSpace(confirmPassword))
                {
                    errors.Add("Подтверждение пароля не может быть пустым");
                }
                else if (password != confirmPassword)
                {
                    errors.Add("Пароли не совпадают");
                }
            }
            return errors;
        }
    }
}
