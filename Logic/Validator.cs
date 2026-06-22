namespace Logic
{
    public static class Validator // класс проверки логина и пароля
    {
        public static List<string> ValidateCredentials(string login, string password, string confirmPassword = null, bool isRegistration = false)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(login))            
                errors.Add("Логин не может быть пустым");            
            else if (login.Length < 5 || login.Length > 50)            
                errors.Add("Логин должен быть от 5 до 50 символов");         

            if (string.IsNullOrWhiteSpace(password))            
                errors.Add("Пароль не может быть пустым");            
            else if (password.Length < 5)
                errors.Add("Пароль должен содержать не менее 5 символов.");

            if (isRegistration)
            {
                if (string.IsNullOrWhiteSpace(confirmPassword))                
                    errors.Add("Подтверждение пароля не может быть пустым");                
                else if (password != confirmPassword)                
                    errors.Add("Пароли не совпадают");                
            }
            return errors;
        }
    }
}
