using System.Drawing;
using System.Windows.Forms;

namespace PractiStudent
{
    public static class UIStyles
    {
        public static readonly Color PrimaryButton = Color.FromArgb(230, 235, 240);
        public static readonly Color SuccessButton = Color.FromArgb(220, 240, 220);
        public static readonly Color DangerButton = Color.FromArgb(245, 220, 220);
        public static readonly Color NeutralButton = Color.FromArgb(240, 240, 240);
        public static readonly Color Background = Color.FromArgb(245, 245, 245);
        public static readonly Color TextColor = Color.FromArgb(45, 45, 45);
        public static readonly Color HintColor = Color.Gray;

        public static readonly Font TitleFont = new Font("Segoe UI", 14, FontStyle.Bold);
        public static readonly Font SubtitleFont = new Font("Segoe UI", 12, FontStyle.Bold);
        public static readonly Font ButtonFont = new Font("Segoe UI", 10);
        public static readonly Font LabelFont = new Font("Segoe UI", 9);
        public static readonly Font InputFont = new Font("Segoe UI", 11);
        public static readonly Font HintFont = new Font("Segoe UI", 8);

        // настройки размеров
        public static readonly Size ButtonSize = new Size(280, 45);
        public static readonly Size InputSize = new Size(280, 25);
        public static readonly Size LabelSize = new Size(280, 15);
        public static readonly Size FormSize = new Size(400, 450);
        public static readonly Size MainFormSize = new Size(1200, 700);

        // настройки форм
        public static readonly FormStartPosition DefaultFormStartPosition = FormStartPosition.CenterScreen;
        public static readonly FormBorderStyle DefaultBorderStyle = FormBorderStyle.FixedSingle;
        public static readonly bool AllowMaximize = false;
        public static readonly bool AllowMinimize = false;

        public static readonly string LoginFormTitle = "Авторизация в системе";
        public static readonly string MainFormTitleAdmin = "Главное окно (Администратор)";
        public static readonly string MainFormTitleGuest = "Главное окно (Гость)";

        public static readonly string DatabaseRequiredTitle = "Система \"Абитуриент\"";
        public static readonly string DatabaseRequiredMessage = "Требуется подключение\nк базе данных";
        public static readonly string ChooseActionMessage = "Выберите действие";
        public static readonly string DatabaseFilter = "Access Database Files|*.accdb;*.mdb|All Files|*.*";
        public static readonly string DatabaseTitle = "Выберите базу данных \"Абитуриент\"";
        public static readonly string DatabaseConnectionPrefix = "Подключение: ";

        public static readonly string AddRecordTitle = "Добавление новой записи";
        public static readonly string EditRecordTitle = "Редактирование записи";
        public static readonly string AutoValueText = "(автоматически)";
        public static readonly string NotSelectedText = "(не выбрано)";
        public static readonly string AllValuesText = "(Все значения)";
        public static readonly string FilterHint = "Можно вводить часть значения для поиска";

        // экран подключения
        public static readonly Point InitialTitlePosition = new Point(20, 80);
        public static readonly Size InitialTitleSize = new Size(345, 100);
        public static readonly Point ButtonConnectPosition = new Point(50, 210);
        public static readonly Point ButtonExitInitialPosition = new Point(50, 310);
        public static readonly Point ConnectionStatusPosition = new Point(10, 8);
        public static readonly Size ConnectionStatusSize = new Size(380, 30);

        // главное меню
        public static readonly Point MenuTitlePosition = new Point(20, 30);
        public static readonly Size MenuTitleSize = new Size(345, 60);
        public static readonly Point ButtonRegisterPosition = new Point(50, 120);
        public static readonly Point ButtonLoginGuestPosition = new Point(50, 180);
        public static readonly Point ButtonLoginAdminPosition = new Point(50, 240);
        public static readonly Point ButtonExitPosition = new Point(50, 320);

        // панель ввода
        public static readonly Point InputPanelPosition = new Point(20, 20);
        public static readonly Size InputPanelSize = new Size(345, 370);
        public static readonly Point FormActionTitlePosition = new Point(0, 10);
        public static readonly Size FormActionTitleSize = new Size(345, 30);
        public static readonly Point LoginLabelPosition = new Point(30, 55);
        public static readonly Point LoginInputPosition = new Point(30, 75);
        public static readonly Point PasswordLabelPosition = new Point(30, 115);
        public static readonly Point PasswordInputPosition = new Point(30, 135);
        public static readonly Point ConfirmPasswordLabelPosition = new Point(30, 175);
        public static readonly Point ConfirmPasswordInputPosition = new Point(30, 195);
        public static readonly Point ButtonSubmitPosition = new Point(30, 250);
        public static readonly Point ButtonBackPosition = new Point(30, 310);

        // левая панель
        public static readonly Point TableSelectLabelPosition = new Point(15, 35);
        public static readonly Point TableSelectComboPosition = new Point(15, 58);
        public static readonly Size TableSelectComboSize = new Size(280, 30);
        public static readonly Point ConnectionInfoPosition = new Point(10, 5);
        public static readonly Size ConnectionInfoSize = new Size(380, 20);

        // панели действий
        public static readonly int ActionPanelTop = 100;
        public static readonly int ActionPanelHeight = 500;
        public static readonly Size ActionPanelSize = new Size(370, 500);
        public static readonly Point ActionButtonPosition = new Point(0, 10);
        public static readonly int ButtonVerticalSpacing = 50;
        public static readonly Size ActionButtonSize = new Size(280, 40);
 
        public static readonly int LabelTopMargin = 10;
        public static readonly int ControlSpacing = 15;
        public static readonly int PanelLeftPadding = 20;
        public static readonly int PanelTopPadding = 20;
        public static readonly int ButtonBottomMargin = 60;

        public static readonly Color DataGridViewHeaderBackColor = Color.FromArgb(230, 235, 240);
        public static readonly Color DataGridViewRowBackColor = Color.White;
        public static readonly Color DataGridViewAltRowBackColor = Color.FromArgb(248, 248, 248);
        public static readonly Color DataGridViewGridColor = Color.FromArgb(220, 220, 220);
        public static readonly Font DataGridViewFont = new Font("Segoe UI", 9);
        public static readonly Font DataGridViewHeaderFont = new Font("Segoe UI", 9, FontStyle.Bold);
    }
}