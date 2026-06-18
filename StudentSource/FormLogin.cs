using Logic;
using StudentSource;

namespace PractiStudent
{
    public partial class FormLogin : Form
    {
        private readonly UserService _userService;
        private bool _isDatabaseConnected = false;

        private Button btnConnectDatabase, btnExitInitial, btnRegisterGuest, btnLoginGuest, btnLoginAdmin, btnExit;
        private Label lblInitialTitle, lblConnectionStatus, lblTitle, lblFormActionTitle;
        private Panel panelInputContainer;
        private TextBox txtLogin, txtPassword, txtConfirmPassword;
        private Button btnSubmitAction, btnBackToMenu;

        private string currentMode = "";
        protected override CreateParams CreateParams // Переопределние свойств создания окна
        {
            get
            {
                CreateParams cp = base.CreateParams;
                const int CS_NOCLOSE = 0x200; // Убрать кнопку закрытия формы
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }
        }
        public FormLogin() // Инициализация формы и всего сопутствующего
        {
            InitializeComponent();
            _userService = new UserService(); 
            ConfigureForm(); // Внешний вид
            InitializeAllComponents(); 
            ShowConnectionScreen(); // Для показа самой 1 вариации
        }
        private void ConfigureForm()
        {
            this.Text = UIStyles.LoginFormTitle; // Вся косметика ставится в классе UIStyles
            this.Size = UIStyles.FormSize;
            this.StartPosition = UIStyles.DefaultFormStartPosition;
            this.FormBorderStyle = UIStyles.DefaultBorderStyle;
            this.MaximizeBox = UIStyles.AllowMaximize;
            this.MinimizeBox = UIStyles.AllowMinimize;
            this.BackColor = UIStyles.Background;
        }

        private void InitializeAllComponents()
        {
            InitializeConnectionScreen(); // Требуется подключение
            InitializeCustomComponents(); // Выбор под кем войти
            InitializeDynamicInputPanel(); // Ввод логина и пароля
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

        private void InitializeConnectionScreen()
        {
            lblInitialTitle = new Label
            {
                Text = $"{UIStyles.DatabaseRequiredTitle}\n\n{UIStyles.DatabaseRequiredMessage}",
                Font = UIStyles.TitleFont,
                ForeColor = UIStyles.TextColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = UIStyles.InitialTitlePosition,
                Size = UIStyles.InitialTitleSize
            };

            btnConnectDatabase = CreateStyledButton("Подключиться к БД", UIStyles.ButtonConnectPosition,
                UIStyles.PrimaryButton, BtnConnectDatabase_Click);
            btnExitInitial = CreateStyledButton("Выйти из приложения", UIStyles.ButtonExitInitialPosition,
                UIStyles.DangerButton, BtnExit_Click);

            lblConnectionStatus = new Label
            {
                Text = "",
                Font = UIStyles.HintFont,
                ForeColor = UIStyles.TextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = UIStyles.ConnectionStatusPosition,
                Size = UIStyles.ConnectionStatusSize,
                Visible = false
            };

            this.Controls.AddRange(new Control[] { lblInitialTitle, btnConnectDatabase, btnExitInitial, lblConnectionStatus });
        }

        private void InitializeCustomComponents()
        {
            lblTitle = new Label
            {
                Text = "Система \"Абитуриент\"\nВыберите действие",
                Font = UIStyles.TitleFont,
                ForeColor = UIStyles.TextColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 30),
                Size = new Size(345, 60)
            };

            btnRegisterGuest = CreateStyledButton("Зарегистрироваться как гость", new Point(50, 120), UIStyles.PrimaryButton, BtnRegisterGuest_Click);
            btnLoginGuest = CreateStyledButton("Войти как гость", new Point(50, 180), UIStyles.PrimaryButton, (s, e) => ToggleFormScreen(true, "LoginGuest"));
            btnLoginAdmin = CreateStyledButton("Войти как администратор", new Point(50, 240), UIStyles.PrimaryButton, (s, e) => ToggleFormScreen(true, "LoginAdmin"));
            btnExit = CreateStyledButton("Выйти из приложения", new Point(50, 320), UIStyles.DangerButton, BtnExit_Click);

            lblTitle.Visible = false;
            btnRegisterGuest.Visible = false;
            btnLoginGuest.Visible = false;
            btnLoginAdmin.Visible = false;
            btnExit.Visible = false;

            this.Controls.AddRange(new Control[] { lblTitle, btnRegisterGuest, btnLoginGuest, btnLoginAdmin, btnExit });
        }

        private void InitializeDynamicInputPanel()
        {
            panelInputContainer = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(345, 370),
                Visible = false
            };

            lblFormActionTitle = new Label
            {
                Font = UIStyles.TitleFont,
                ForeColor = UIStyles.TextColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 10),
                Size = new Size(345, 30)
            };

            Label lblLogin = new Label { Text = "Логин:", Font = UIStyles.LabelFont, Location = new Point(30, 55), Size = new Size(280, 15) };
            txtLogin = new TextBox { Font = UIStyles.InputFont, Location = new Point(30, 75), Size = UIStyles.InputSize };

            Label lblPassword = new Label { Text = "Пароль:", Font = UIStyles.LabelFont, Location = new Point(30, 115), Size = new Size(280, 15) };
            txtPassword = new TextBox { Font = UIStyles.InputFont, Location = new Point(30, 135), Size = UIStyles.InputSize, UseSystemPasswordChar = true };

            Label lblConfirmPassword = new Label { Name = "lblConfirmPassword", Text = "Повторите пароль:", Font = UIStyles.LabelFont, Location = new Point(30, 175), Size = new Size(280, 15) };
            txtConfirmPassword = new TextBox { Name = "txtConfirmPassword", Font = UIStyles.InputFont, Location = new Point(30, 195), Size = UIStyles.InputSize, UseSystemPasswordChar = true };

            btnSubmitAction = CreateStyledButton("Подтвердить", new Point(30, 250), UIStyles.SuccessButton, BtnSubmitAction_Click);
            btnBackToMenu = CreateStyledButton("Вернуться назад", new Point(30, 310), UIStyles.NeutralButton, BtnBackToMenu_Click);

            panelInputContainer.Controls.AddRange(new Control[] {
                lblFormActionTitle, lblLogin, txtLogin, lblPassword, txtPassword,
                lblConfirmPassword, txtConfirmPassword, btnSubmitAction, btnBackToMenu
            });

            this.Controls.Add(panelInputContainer);
        }

        private void ShowConnectionScreen()
        {
            _isDatabaseConnected = false;
            lblInitialTitle.Visible = true;
            btnConnectDatabase.Visible = true;
            btnExitInitial.Visible = true;
            lblConnectionStatus.Visible = false;
            lblTitle.Visible = false;
            btnRegisterGuest.Visible = false;
            btnLoginGuest.Visible = false;
            btnLoginAdmin.Visible = false;
            btnExit.Visible = false;
            panelInputContainer.Visible = false;
        }

        private void ShowAuthorizationScreen()
        {
            _isDatabaseConnected = true;

            lblInitialTitle.Visible = false;
            btnConnectDatabase.Visible = false;
            btnExitInitial.Visible = false;

            lblConnectionStatus.Visible = true;
            lblConnectionStatus.Text = $" Подключение: \"{_userService.GetDatabaseFileName()}\"";
            lblConnectionStatus.Location = new Point(10, 8);
            lblConnectionStatus.Size = new Size(380, 30);

            lblTitle.Visible = true;
            btnRegisterGuest.Visible = true;
            btnLoginGuest.Visible = true;
            btnLoginAdmin.Visible = true;
            btnExit.Visible = true;
        }

        private void BtnConnectDatabase_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = UIStyles.DatabaseFilter;
                openFileDialog.Title = UIStyles.DatabaseTitle;
                openFileDialog.CheckFileExists = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;

                    if (_userService.ConnectToDatabase(selectedFilePath))
                    {
                        ShowAuthorizationScreen();
                    }
                    else
                    {
                        ErrorHandler.ShowWarning(
                            "Не удалось подключиться к базе данных.\n\n" +
                            "Убедитесь, что:\n" +
                            "1. Файл является базой данных Access (.accdb или .mdb)\n" +
                            "2. Файл содержит таблицу \"Пользователи\"\n" +
                            "3. Файл не поврежден и не защищен паролем");
                    }
                }
            }
        }

        private void ToggleFormScreen(bool showInputFields, string mode = "")
        {
            currentMode = mode;

            lblTitle.Visible = !showInputFields && _isDatabaseConnected;
            btnRegisterGuest.Visible = !showInputFields && _isDatabaseConnected;
            btnLoginGuest.Visible = !showInputFields && _isDatabaseConnected;
            btnLoginAdmin.Visible = !showInputFields && _isDatabaseConnected;
            btnExit.Visible = !showInputFields && _isDatabaseConnected;

            txtLogin.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();

            if (showInputFields)
            {
                if (mode == "Register")
                {
                    lblFormActionTitle.Text = "Регистрация гостя";
                    panelInputContainer.Controls["lblConfirmPassword"].Visible = true;
                    txtConfirmPassword.Visible = true;
                    btnSubmitAction.Location = new Point(30, 250);
                }
                else
                {
                    lblFormActionTitle.Text = mode == "LoginAdmin" ? "Вход для администратора" : "Вход в систему";
                    panelInputContainer.Controls["lblConfirmPassword"].Visible = false;
                    txtConfirmPassword.Visible = false;
                    btnSubmitAction.Location = new Point(30, 185);
                }
                panelInputContainer.Visible = true;
            }
            else
            {
                panelInputContainer.Visible = false;
            }
        }

        private void BtnRegisterGuest_Click(object sender, EventArgs e)
        {
            ToggleFormScreen(true, "Register");
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnBackToMenu_Click(object sender, EventArgs e)
        {
            ToggleFormScreen(false);
        }

        private void PerformLogin(string role)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorHandler.ShowWarning("Пожалуйста, заполните все поля!");
                return;
            }

            try
            {
                string userRole = _userService.ValidateUser(login, password, role);

                if (userRole != null)
                {
                    ErrorHandler.ShowInfo($"Вы успешно вошли как {role.ToLower()}!");
                    this.Hide();
                    FormMain mainForm = new FormMain(userRole, null, _userService.GetDatabaseFileName(), _userService);
                    mainForm.ShowDialog();
                    this.Show();
                }
                else
                {
                    ErrorHandler.ShowWarning($"Неверный логин или пароль для {role.ToLower()}!");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, $"Login as {role}");
            }
        }
        private void BtnSubmitAction_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (!Validator.ValidateLogin(login, out string loginError))
            {
                ErrorHandler.ShowWarning(loginError);
                return;
            }

            if (!Validator.ValidatePassword(password, out string passwordError))
            {
                ErrorHandler.ShowWarning(passwordError);
                return;
            }

            if (currentMode == "Register")
            {
                string confirmPassword = txtConfirmPassword.Text.Trim();
                if (!Validator.ValidatePasswordsMatch(password, confirmPassword, out string matchError))
                {
                    ErrorHandler.ShowWarning(matchError);
                    return;
                }
                try
                {
                    if (_userService.RegisterGuest(login, password))
                    {
                        ErrorHandler.ShowInfo($"Регистрация гостя '{login}' успешна!");
                        ToggleFormScreen(false);
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.Handle(ex, "Register Guest");
                }
            }
            else if (currentMode == "LoginGuest")
            {
                PerformLogin("Гость");
            }
            else if (currentMode == "LoginAdmin")
            {
                PerformLogin("Администратор");
            }
        }
    }
}