using System;
using System.Windows.Forms;
namespace PractiStudent
{
    public partial class FormMain : Form
    {
        private string currentUserRole;
        private int? currentRegNumber;

        public FormMain(string role, int? regNumber)
        {
            InitializeComponent();

            this.currentUserRole = role;
            this.currentRegNumber = regNumber;

            ApplyUserPermissions();
        }

        private void ApplyUserPermissions()
        {            
            if (currentUserRole == "Абитуриент") // Если вошел гость — блокируем элементы управления
            {
                this.Text = $"Главное окно (Режим просмотра: Абитуриент №{currentRegNumber})";

                //btnDelete.Enabled = false; // Отключаем кнопки модификации данных 
                //btnEdit.Enabled = false;
                //btnAdd.Enabled = false;

                // dataGridView1.ReadOnly = true; 
            }
            else if (currentUserRole == "Администратор")
            {
                this.Text = "Главное окно (Режим разработчика: Администратор)";
            }
        }
    }

}
