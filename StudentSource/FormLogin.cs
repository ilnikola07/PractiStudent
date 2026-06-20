using Logic;
using StudentSource;

namespace PractiStudent
{
    public partial class FormLogin : Form
    {        
        private const string ModeRegister = "Register"; // Константы режимов работы формы
        private const string ModeLoginGuest = "LoginGuest";
        private const string ModeLoginAdmin = "LoginAdmin";

        private readonly UserService _userService;
        private bool _isDatabaseConnected = false;
        
        private Button btnConnectDatabase, btnExitInitial; // Элементы экрана подключения к БД
        private Label lblInitialTitle, lblConnectionStatus;
                
        private Button btnRegisterGuest, btnLoginGuest, btnLoginAdmin, btnExit;
        private Label lblTitle; // Элементы главного меню авторизации
                
        private Panel panelInputContainer; // Элементы панели ввода учётных данных
        private TextBox txtLogin, txtPassword, txtConfirmPassword;
        private Button btnSubmitAction, btnBackToMenu;
        private Label lblFormActionTitle;

        private string currentMode = "";
               
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x200; // Убрать крестик для закрытия
                return cp; 
            }
        }
        public FormLogin()
        {
            InitializeComponent();
            _userService = new UserService();
            ConfigureForm();
            InitializeAllComponents();
            ShowConnectionScreen();
        }
        private void ConfigureForm() // Настройка внешнего вида формы
        {
            this.Text = UIStyles.LoginFormTitle;
            this.Size = UIStyles.FormSize;
            this.StartPosition = UIStyles.DefaultFormStartPosition;
            this.FormBorderStyle = UIStyles.DefaultBorderStyle;
            this.MaximizeBox = UIStyles.AllowMaximize;
            this.MinimizeBox = UIStyles.AllowMinimize;
            this.BackColor = UIStyles.Background;
        }       
        private void InitializeAllComponents()
        {
            InitializeConnectionScreen();
            InitializeMainMenu();
            InitializeInputPanel();
        }        
        private Button CreateStyledButton(string text, Point location, Color backColor, EventHandler clickEvent)
        {
            Button btn = new Button
            {
                Text = text,
                Font = UIStyles.ButtonFont,
                Location = location,
                Size = UIStyles.ButtonSize,
                BackColor = backColor,
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += clickEvent;
            return btn;
        }        
        private void InitializeConnectionScreen() // Инициализация экрана подключения к бд
        {
            lblInitialTitle = CreateInitialTitle();
            btnConnectDatabase = CreateConnectButton();
            btnExitInitial = CreateExitButton();
            lblConnectionStatus = CreateConnectionStatusLabel();
            this.Controls.AddRange(new Control[] { lblInitialTitle, btnConnectDatabase, btnExitInitial, lblConnectionStatus });
        }        
        private Label CreateInitialTitle() // Создание заголовка экрана подключения
        {
            return new Label
            {
                Text = $"{UIStyles.DatabaseRequiredTitle}\n\n{UIStyles.DatabaseRequiredMessage}",
                Font = UIStyles.TitleFont,
                ForeColor = UIStyles.TextColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = UIStyles.InitialTitlePosition,
                Size = UIStyles.InitialTitleSize
            };
        }        
        private Button CreateConnectButton() // Создание кнопки подключения к бд
        {
            return CreateStyledButton("Подключиться к БД", UIStyles.ButtonConnectPosition,
                UIStyles.PrimaryButton, BtnConnectDatabase_Click);
        }        
        private Button CreateExitButton() // Создание кнопки выхода из приложения
        {
            return CreateStyledButton("Выйти из приложения", UIStyles.ButtonExitInitialPosition,
                UIStyles.DangerButton, BtnExit_Click);
        }        
        private Label CreateConnectionStatusLabel() // Метка статуса подключения
        {
            return new Label
            {
                Text = "",
                Font = UIStyles.HintFont,
                ForeColor = UIStyles.TextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = UIStyles.ConnectionStatusPosition,
                Size = UIStyles.ConnectionStatusSize,
                Visible = false
            };
        }        
        private void InitializeMainMenu() // Инициализация главного меню 
        {
            lblTitle = CreateMainMenuTitle();
            btnRegisterGuest = CreateRegisterButton();
            btnLoginGuest = CreateLoginGuestButton();
            btnLoginAdmin = CreateLoginAdminButton();
            btnExit = CreateExitButton();

            this.Controls.AddRange(new Control[] { lblTitle, btnRegisterGuest, btnLoginGuest, btnLoginAdmin, btnExit });
        }        
        private Label CreateMainMenuTitle() // Создание заголовка главного меню
        {
            return new Label
            {
                Text = "Система \"Абитуриент\"\nВыберите действие",
                Font = UIStyles.TitleFont,
                ForeColor = UIStyles.TextColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 30),
                Size = new Size(345, 60)
            };
        }        
        private Button CreateRegisterButton() // Создание кнопки регистрации гостя
        {
            return CreateStyledButton("Зарегистрироваться как гость", new Point(50, 120),
                UIStyles.PrimaryButton, BtnRegisterGuest_Click);
        }        
        private Button CreateLoginGuestButton() // Создание кнопки входа как гость
        {
            return CreateStyledButton("Войти как гость", new Point(50, 180),
                UIStyles.PrimaryButton, (s, e) => ShowInputScreen(ModeLoginGuest));
        }        
        private Button CreateLoginAdminButton() // Создание кнопки входа как администратор
        {
            return CreateStyledButton("Войти как администратор", new Point(50, 240),
                UIStyles.PrimaryButton, (s, e) => ShowInputScreen(ModeLoginAdmin));
        }        
        private void InitializeInputPanel() // Инициализация панели ввода учётных данных
        {
            panelInputContainer = CreateInputPanel();
            lblFormActionTitle = CreateFormActionTitle();
            txtLogin = CreateLoginTextBox();
            txtPassword = CreatePasswordTextBox();
            txtConfirmPassword = CreateConfirmPasswordTextBox();
            btnSubmitAction = CreateSubmitButton();
            btnBackToMenu = CreateBackButton();

            AddControlsToInputPanel();
            this.Controls.Add(panelInputContainer);
        }       
        private Panel CreateInputPanel() // Создание контейнера для панели ввода
        {
            return new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(345, 370),
                Visible = false
            };
        }        
        private Label CreateFormActionTitle() // Создание заголовка панели действий
        {
            return new Label
            {
                Font = UIStyles.TitleFont,
                ForeColor = UIStyles.TextColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 10),
                Size = new Size(345, 30)
            };
        }        
        private TextBox CreateLoginTextBox() // Создание поля ввода логина
        {
            Label lblLogin = new Label { Text = "Логин:", Font = UIStyles.LabelFont, Location = new Point(30, 55), Size = new Size(280, 15) };
            TextBox txtLogin = new TextBox { Font = UIStyles.InputFont, Location = new Point(30, 75), Size = UIStyles.InputSize };
            panelInputContainer.Controls.Add(lblLogin);
            return txtLogin;
        }        
        private TextBox CreatePasswordTextBox() // Создание поля ввода пароля
        {
            Label lblPassword = new Label { Text = "Пароль:", Font = UIStyles.LabelFont, Location = new Point(30, 115), Size = new Size(280, 15) };
            TextBox txtPassword = new TextBox { Font = UIStyles.InputFont, Location = new Point(30, 135), Size = UIStyles.InputSize, UseSystemPasswordChar = true };
            panelInputContainer.Controls.Add(lblPassword);
            return txtPassword;
        }       
        private TextBox CreateConfirmPasswordTextBox()  // Создание поля подтверждения пароля
        {
            Label lblConfirmPassword = new Label { Name = "lblConfirmPassword", Text = "Повторите пароль:", Font = UIStyles.LabelFont, Location = new Point(30, 175), Size = new Size(280, 15) };
            TextBox txtConfirmPassword = new TextBox { Name = "txtConfirmPassword", Font = UIStyles.InputFont, Location = new Point(30, 195), Size = UIStyles.InputSize, UseSystemPasswordChar = true };
            panelInputContainer.Controls.Add(lblConfirmPassword);
            return txtConfirmPassword;
        }        
        private Button CreateSubmitButton() // Создание кнопки подтверждения
        {
            return CreateStyledButton("Подтвердить", new Point(30, 250), UIStyles.SuccessButton, BtnSubmitAction_Click);
        }       
        private Button CreateBackButton() // Создание кнопки возврата в меню
        {
            return CreateStyledButton("Вернуться назад", new Point(30, 310), UIStyles.NeutralButton, (s, e) => ShowMainMenuScreen());
        }        
        private void AddControlsToInputPanel() // Добавление всех элементов на панель ввода
        {
            panelInputContainer.Controls.AddRange(new Control[] {
                lblFormActionTitle, txtLogin, txtPassword,
                txtConfirmPassword, btnSubmitAction, btnBackToMenu
            });
        }        
        private void ShowConnectionScreen() // Показ экрана подключения к БД
        {
            _isDatabaseConnected = false;
            HideAllControls();
            lblInitialTitle.Visible = true;
            btnConnectDatabase.Visible = true;
            btnExitInitial.Visible = true;
        }        
        private void ShowMainMenuScreen() // Показ главного меню авторизации
        {
            if (!_isDatabaseConnected) return;

            HideAllControls();
            lblConnectionStatus.Visible = true;
            UpdateConnectionStatus();
            lblTitle.Visible = true;
            btnRegisterGuest.Visible = true;
            btnLoginGuest.Visible = true;
            btnLoginAdmin.Visible = true;
            btnExit.Visible = true;
        }        
        private void ShowInputScreen(string mode) // Показ экрана ввода учётных данных
        {
            if (!_isDatabaseConnected) return;

            currentMode = mode;
            HideAllControls();
            ClearInputFields();
            ConfigureInputPanelForMode(mode);
            panelInputContainer.Visible = true;
        }        
        private void HideAllControls() // Скрытие всех элементов управления
        {
            lblInitialTitle.Visible = false;
            btnConnectDatabase.Visible = false;
            btnExitInitial.Visible = false;
            lblConnectionStatus.Visible = false;
            lblTitle.Visible = false;
            btnRegisterGuest.Visible = false;
            btnLoginGuest.Visible = false;
            btnLoginAdmin.Visible = false;
            btnExit.Visible = false;
            panelInputContainer.Visible = false;
        }        
        private void UpdateConnectionStatus() // Обновление статуса подключения
        {
            lblConnectionStatus.Text = $" Подключение: \"{_userService.GetDatabaseFileName()}\"";
        }        
        private void ClearInputFields() // Очистка полей ввода
        {
            txtLogin.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
        }        
        private void ConfigureInputPanelForMode(string mode) // Настройка панели ввода для выбранного режима
        {
            if (mode == ModeRegister)
            {
                ConfigureForRegistration();
            }
            else
            {
                ConfigureForLogin(mode);
            }
        }        
        private void ConfigureForRegistration() // Настройка панели для режима регистрации
        {
            lblFormActionTitle.Text = "Регистрация гостя";
            panelInputContainer.Controls["lblConfirmPassword"].Visible = true;
            txtConfirmPassword.Visible = true;
            btnSubmitAction.Location = new Point(30, 250);
        }        
        private void ConfigureForLogin(string mode) // Настройка панели для режима входа
        {
            lblFormActionTitle.Text = mode == ModeLoginAdmin ? "Вход для администратора" : "Вход в систему";
            panelInputContainer.Controls["lblConfirmPassword"].Visible = false;
            txtConfirmPassword.Visible = false;
            btnSubmitAction.Location = new Point(30, 185);
        }        
        private void BtnConnectDatabase_Click(object sender, EventArgs e) // Обработчик кнопки подключения к БД
        {
            string selectedFilePath = ShowDatabaseSelectionDialog();

            if (string.IsNullOrEmpty(selectedFilePath)) return;

            if (_userService.ConnectToDatabase(selectedFilePath))
            {
                _isDatabaseConnected = true;
                ShowMainMenuScreen();
            }
            else
            {
                ShowConnectionError();
            }
        }        
        private string ShowDatabaseSelectionDialog() // Показ диалога выбора файла БД
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = UIStyles.DatabaseFilter;
                openFileDialog.Title = UIStyles.DatabaseTitle;
                openFileDialog.CheckFileExists = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
                return null;
            }
        }        
        private void ShowConnectionError() // Показ сообщения об ошибке подключения
        {
            ErrorHandler.ShowWarning(
                "Не удалось подключиться к базе данных.\n\n" +
                "Убедитесь, что:\n" +
                "1. Файл является базой данных Access (.accdb или .mdb)\n" +
                "2. Файл содержит таблицу \"Пользователи\"\n" +
                "3. Файл не поврежден и не защищен паролем");
        }        
        private void BtnRegisterGuest_Click(object sender, EventArgs e) // Обработчик кнопки регистрации гостя
        {
            ShowInputScreen(ModeRegister);
        }        
        private void BtnExit_Click(object sender, EventArgs e) // Обработчик кнопки выхода из приложения
        {
            Application.Exit();
        }       
        private void PerformLogin(string role, string login, string password)  // Авторизация пользователя с проверкой учётных данных
        {
            try
            {
                string userRole = _userService.ValidateUser(login, password, role);

                if (userRole != null)
                {
                    ShowLoginSuccess(role);
                    OpenMainForm(userRole);
                }
                else
                {
                    ShowLoginError(role);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, $"Login as {role}");
            }
        }        
        private void ShowLoginSuccess(string role) // Показ сообщения об успешном входе
        {
            ErrorHandler.ShowInfo($"Вы успешно вошли как {role.ToLower()}!");
        }        
        private void OpenMainForm(string userRole) // Открытие главной формы
        {
            this.Hide();
            FormMain mainForm = new FormMain(userRole, null, _userService.GetDatabaseFileName(), _userService);
            mainForm.ShowDialog();
            ForceCleanupComResources();
            this.Show();
        }
        private void ForceCleanupComResources()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                System.Threading.Thread.Sleep(100);
            }
            catch { }
        }
        private void ShowLoginError(string role) // Показ сообщения об ошибке входа
        {
            ErrorHandler.ShowWarning($"Неверный логин или пароль для {role.ToLower()}!");
        }       
        private void BtnSubmitAction_Click(object sender, EventArgs e)  // Обработчик кнопки подтверждения действий
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            var errors = Validator.ValidateCredentials(login, password, confirmPassword, currentMode == ModeRegister);

            if (errors.Count > 0)
            {
                ErrorHandler.ShowWarning(string.Join("\n", errors));
                return;
            }

            ProcessAction(login, password);
        }       
        private void ProcessAction(string login, string password)  // Обработка выбранного действия
        {
            if (currentMode == ModeRegister)
            {
                RegisterNewGuest(login, password);
            }
            else if (currentMode == ModeLoginGuest)
            {
                PerformLogin("Гость", login, password);
            }
            else if (currentMode == ModeLoginAdmin)
            {
                PerformLogin("Администратор", login, password);
            }
        }        
        private void RegisterNewGuest(string login, string password) // Регистрация нового гостя
        {
            try
            {
                if (_userService.RegisterGuest(login, password))
                {
                    ErrorHandler.ShowInfo($"Регистрация гостя '{login}' успешна!");
                    ShowMainMenuScreen();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "Register Guest");
            }
        }
    }
}