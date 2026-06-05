using Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PractiStudent
{
    public partial class FormLogin : Form
    {
        private readonly UserService _userService;
                
        private Button btnRegisterGuest; // Изначальные элементы формы
        private Button btnLoginGuest;
        private Button btnLoginAdmin;
        private Button btnExit;
        private Label lblTitle;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                const int CS_NOCLOSE = 0x200; // Отключает кнопку закрытия 
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

            InitializeCustomComponents();
        }
        
        private Button CreateStyledButton(string text, Point location, Color backColor, EventHandler clickEvent) // Для создания кнопок в едином стиле
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

        private void InitializeCustomComponents()
        {            
            lblTitle = new Label // Заголовок 
            {
                Text = "Система \"Абитуриент\"\nВыберите действие",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 45),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 30),
                Size = new Size(345, 60)
            };
            
            Color actionButtonColor = Color.FromArgb(230, 235, 240); // Цвет для кнопок 
            Color exitButtonColor = Color.FromArgb(245, 220, 220);
                        
            btnRegisterGuest = CreateStyledButton("Зарегистрироваться как гость", new Point(50, 120), actionButtonColor, BtnRegisterGuest_Click); // Кнопки через универсальный метод
            btnLoginGuest = CreateStyledButton("Войти как гость", new Point(50, 180), actionButtonColor, BtnLoginGuest_Click);
            btnLoginAdmin = CreateStyledButton("Войти как администратор", new Point(50, 240), actionButtonColor, BtnLoginAdmin_Click);
            btnExit = CreateStyledButton("Выйти из приложения", new Point(50, 320), exitButtonColor, BtnExit_Click);

            this.Controls.AddRange(new Control[] { lblTitle, btnRegisterGuest, btnLoginGuest, btnLoginAdmin, btnExit });
        }

        private void BtnRegisterGuest_Click(object? sender, EventArgs e)
        {
            // Здесь будет логика для регистрации гостя
        }

        private void BtnLoginGuest_Click(object? sender, EventArgs e)
        {
            // Здесь будет логика для входа гостя
        }

        private void BtnLoginAdmin_Click(object? sender, EventArgs e)
        {
            // Здесь будет логика для входа админа
        }

        private void BtnExit_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
