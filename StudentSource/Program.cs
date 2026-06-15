namespace PractiStudent
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FormLogin loginForm = new FormLogin();
            Application.Run(loginForm); // FormLogin теперь главная форма приложения

            //Application.Run(new FormLogin()); // Первой запускается форма авторизации
        }
    }
}