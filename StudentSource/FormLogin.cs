using Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PractiStudent
{
    public partial class FormLogin : Form
    {
        private readonly UserService _userService;
        private bool _isDatabaseConnected = false;

        private Button btnConnectDatabase;
        private Button btnExitInitial;
        private Label lblInitialTitle;
        private Label lblConnectionStatus;

        private Button btnRegisterGuest;
        private Button btnLoginGuest;
        private Button btnLoginAdmin;
        private Button btnExit;
        private Label lblTitle;

        private Panel panelInputContainer;
        private Label lblFormActionTitle;
        private TextBox txtLogin;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private Button btnSubmitAction;
        private Button btnBackToMenu;

        private string currentMode = "";

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                const int CS_NOCLOSE = 0x200;
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }
        }

        public FormLogin()
        {
            InitializeComponent();
            _userService = new UserService();

            this.Text = "Авторизация в системе";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);

            InitializeConnectionScreen();
            InitializeCustomComponents();
            InitializeDynamicInputPanel();

            ShowConnectionScreen();
        }

        private Button CreateStyledButton(string text, Point location, Color backColor, EventHandler clickEvent)
        {
            Button btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                Location = location,
                Size = new Size(280, 45),
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
                Text = "Система \"Абитуриент\"\n\nТребуется подключение\nк базе данных",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 45),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 80),
                Size = new Size(345, 100)
            };

            Color connectButtonColor = Color.FromArgb(230, 235, 240);
            Color exitButtonColor = Color.FromArgb(245, 220, 220);

            btnConnectDatabase = CreateStyledButton("Подключиться к БД", new Point(50, 210), connectButtonColor, BtnConnectDatabase_Click);
            btnExitInitial = CreateStyledButton("Выйти из приложения", new Point(50, 310), exitButtonColor, BtnExit_Click);

            lblConnectionStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(45, 45, 45),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(10, 8),
                Size = new Size(380, 30),
                Visible = false
            };

            this.Controls.AddRange(new Control[] { lblInitialTitle, btnConnectDatabase, btnExitInitial, lblConnectionStatus });
        }

        private void InitializeCustomComponents()
        {
            lblTitle = new Label
            {
                Text = "Система \"Абитуриент\"\nВыберите действие",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 45),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 30),
                Size = new Size(345, 60)
            };

            Color actionButtonColor = Color.FromArgb(230, 235, 240);
            Color exitButtonColor = Color.FromArgb(245, 220, 220);

            btnRegisterGuest = CreateStyledButton("Зарегистрироваться как гость", new Point(50, 120), actionButtonColor, BtnRegisterGuest_Click);
            btnLoginGuest = CreateStyledButton("Войти как гость", new Point(50, 180), actionButtonColor, BtnLoginGuest_Click);
            btnLoginAdmin = CreateStyledButton("Войти как администратор", new Point(50, 240), actionButtonColor, BtnLoginAdmin_Click);
            btnExit = CreateStyledButton("Выйти из приложения", new Point(50, 320), exitButtonColor, BtnExit_Click);

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
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 45),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 10),
                Size = new Size(345, 30)
            };

            Label lblLogin = new Label { Text = "Логин:", Font = new Font("Segoe UI", 9), Location = new Point(30, 55), Size = new Size(280, 15) };
            txtLogin = new TextBox { Font = new Font("Segoe UI", 11), Location = new Point(30, 75), Size = new Size(280, 25) };

            Label lblPassword = new Label { Text = "Пароль:", Font = new Font("Segoe UI", 9), Location = new Point(30, 115), Size = new Size(280, 15) };
            txtPassword = new TextBox { Font = new Font("Segoe UI", 11), Location = new Point(30, 135), Size = new Size(280, 25), UseSystemPasswordChar = true };

            Label lblConfirmPassword = new Label { Name = "lblConfirmPassword", Text = "Повторите пароль:", Font = new Font("Segoe UI", 9), Location = new Point(30, 175), Size = new Size(280, 15) };
            txtConfirmPassword = new TextBox { Name = "txtConfirmPassword", Font = new Font("Segoe UI", 11), Location = new Point(30, 195), Size = new Size(280, 25), UseSystemPasswordChar = true };

            Color submitColor = Color.FromArgb(220, 240, 220);
            Color backColor = Color.FromArgb(240, 240, 240);

            btnSubmitAction = CreateStyledButton("Подтвердить", new Point(30, 250), submitColor, BtnSubmitAction_Click);
            btnBackToMenu = CreateStyledButton("Вернуться назад", new Point(30, 310), backColor, BtnBackToMenu_Click);

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

        private void BtnConnectDatabase_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Access Database Files|*.accdb;*.mdb|All Files|*.*";
                openFileDialog.Title = "Выберите базу данных \"Абитуриент\"";
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
                        MessageBox.Show(
                            "Не удалось подключиться к базе данных.\n\n" +
                            "Убедитесь, что:\n" +
                            "1. Файл является базой данных Access (.accdb или .mdb)\n" +
                            "2. Файл содержит таблицу \"Пользователи\"\n" +
                            "3. Файл не поврежден и не защищен паролем",
                            "Ошибка подключения",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
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

        private void BtnRegisterGuest_Click(object? sender, EventArgs e)
        {
            ToggleFormScreen(true, "Register");
        }

        private void BtnLoginGuest_Click(object? sender, EventArgs e)
        {
            ToggleFormScreen(true, "LoginGuest");
        }

        private void BtnLoginAdmin_Click(object? sender, EventArgs e)
        {
            ToggleFormScreen(true, "LoginAdmin");
        }

        private void BtnExit_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnBackToMenu_Click(object? sender, EventArgs e)
        {
            ToggleFormScreen(false);
        }

        private void BtnSubmitAction_Click(object? sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentMode == "Register")
            {
                string confirmPassword = txtConfirmPassword.Text.Trim();
                if (password != confirmPassword)
                {
                    MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    if (_userService.RegisterGuest(login, password))
                    {
                        MessageBox.Show($"Регистрация гостя '{login}' успешна!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ToggleFormScreen(false);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка регистрации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (currentMode == "LoginGuest")
            {
                try
                {
                    string role = _userService.ValidateUser(login, password, "Гость");

                    if (role != null)
                    {
                        MessageBox.Show("Вы успешно вошли как гость!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Hide();
                        FormMain mainForm = new FormMain(role, null, _userService.GetDatabaseFileName(), _userService);
                        mainForm.ShowDialog();
                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль для гостя!", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Системный сбой", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (currentMode == "LoginAdmin")
            {
                try
                {
                    string role = _userService.ValidateUser(login, password, "Администратор");

                    if (role != null)
                    {
                        MessageBox.Show("Вы успешно вошли как администратор!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Hide();
                        FormMain mainForm = new FormMain(role, null, _userService.GetDatabaseFileName(), _userService);
                        mainForm.ShowDialog();
                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show("Доступ отклонен. Неверный логин или пароль администратора!", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Системный сбой", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}