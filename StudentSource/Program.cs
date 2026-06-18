using StudentSource;

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
            // Глобальный обработчик исключений
            Application.ThreadException += (sender, e) =>
            {
                ErrorHandler.Handle(e.Exception, "Global UI Exception");
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = e.ExceptionObject as Exception;
                ErrorHandler.Handle(ex, "Global AppDomain Exception");
            };


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FormLogin loginForm = new FormLogin();
            Application.Run(loginForm); // FormLogin теперь главная форма приложения

            //Application.Run(new FormLogin()); // Первой запускается форма авторизации
        }
    }
}