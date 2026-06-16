using Logic;
using PractiStudent.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PractiStudent
{
    public partial class FormMain : Form
    {
        private string currentUserRole;
        private int? currentRegNumber;
        private string databaseFileName;
        private UserService _userService;

        private readonly DatabaseHelper _dbHelper;

        private ComboBox cmbTables;
        private Label lblTableSelect;
        private Label lblConnectionInfo;

        private Panel panelMainMenu;
        private Panel panelSearch;
        private Panel panelFilter;
        private Panel panelSort;
        private Panel panelAdd;
        private Panel panelEdit;
        private Panel panelDelete;
        private Panel panelReport;

        private string currentTableName = "";
        private DataTable currentData;
        private string primaryKeyColumn = "";

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

        public FormMain(string role, int? regNumber, string dbFileName, UserService userService)
        {
            InitializeComponent();

            this.currentUserRole = role;
            this.currentRegNumber = regNumber;
            this.databaseFileName = dbFileName;
            this._userService = userService;
            this._dbHelper = new DatabaseHelper();
            this._dbHelper.SetDatabaseConnection(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName));

            this.Text = currentUserRole == "Администратор"
                ? "Главное окно (Администратор)"
                : "Главное окно (Гость)";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);

            // Настройка DataGridView - прокрутка
            dataGridViewMain.ScrollBars = ScrollBars.Both;
            dataGridViewMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            InitializeConnectionIndicator();
            InitializeLeftPanel();
            CreateAllActionPanels();

            ShowPanel(panelMainMenu);
            LoadTablesIntoComboBox();
        }

        private void InitializeConnectionIndicator()
        {
            lblConnectionInfo = new Label
            {
                Text = $"Подключение: {databaseFileName}",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(45, 45, 45),
                Location = new Point(10, 5),
                Size = new Size(380, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
            splitContainer1.Panel1.Controls.Add(lblConnectionInfo);
            lblConnectionInfo.BringToFront();
        }

        private void InitializeLeftPanel()
        {
            lblTableSelect = new Label
            {
                Text = "Выберите таблицу:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 45),
                Location = new Point(15, 35),
                Size = new Size(280, 20)
            };

            cmbTables = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 58),
                Size = new Size(280, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTables.SelectedIndexChanged += CmbTables_SelectedIndexChanged;

            splitContainer1.Panel1.Controls.Add(lblTableSelect);
            splitContainer1.Panel1.Controls.Add(cmbTables);
        }

        private void LoadTablesIntoComboBox()
        {
            try
            {
                List<string> tables = _dbHelper.GetTableNames();
                cmbTables.Items.Clear();

                foreach (string table in tables)
                {
                    if (table.Equals("Пользователи", StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentUserRole == "Администратор")
                        {
                            cmbTables.Items.Add(table);
                        }
                        continue;
                    }

                    cmbTables.Items.Add(table);
                }

                if (cmbTables.Items.Count > 0)
                {
                    cmbTables.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Нет доступных таблиц для просмотра.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки таблиц: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTables.SelectedItem != null)
            {
                currentTableName = cmbTables.SelectedItem.ToString();
                LoadTableData();
            }
        }

        private void LoadTableData()
        {
            if (string.IsNullOrEmpty(currentTableName)) return;

            try
            {
                currentData = _dbHelper.GetAllData(currentTableName);
                dataGridViewMain.DataSource = currentData;

                if (currentData.Columns.Count > 0)
                {
                    primaryKeyColumn = currentData.Columns[0].ColumnName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateAllActionPanels()
        {
            int panelTop = 100;
            int panelHeight = 500;

            panelMainMenu = CreatePanel(panelTop, panelHeight);
            CreateMainMenuButtons();

            panelSearch = CreatePanel(panelTop, panelHeight);
            CreateSearchPanel();

            panelFilter = CreatePanel(panelTop, panelHeight);
            CreateFilterPanel();

            panelSort = CreatePanel(panelTop, panelHeight);
            CreateSortPanel();

            if (currentUserRole == "Администратор")
            {
                panelAdd = CreatePanel(panelTop, panelHeight);
                CreateAddPanel();

                panelEdit = CreatePanel(panelTop, panelHeight);
                CreateEditPanel();

                panelDelete = CreatePanel(panelTop, panelHeight);
                CreateDeletePanel();
            }

            panelReport = CreatePanel(panelTop, panelHeight);
            CreateReportPanel();
        }

        private Panel CreatePanel(int top, int height)
        {
            Panel panel = new Panel
            {
                Location = new Point(15, top),
                Size = new Size(370, height),
                Visible = false,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            splitContainer1.Panel1.Controls.Add(panel);
            return panel;
        }

        private Button CreateButton(string text, Point location, Color backColor, EventHandler click)
        {
            Button btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                Location = location,
                Size = new Size(280, 40),
                BackColor = backColor,
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += click;
            return btn;
        }

        private Label CreateLabel(string text, Point location, bool bold = false)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = Color.FromArgb(45, 45, 45),
                Location = location,
                Size = new Size(280, 20)
            };
        }

        private TextBox CreateTextBox(Point location)
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = location,
                Size = new Size(280, 25)
            };
        }

        private ComboBox CreateComboBox(Point location)
        {
            return new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = location,
                Size = new Size(280, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        private void CreateMainMenuButtons()
        {
            Color btnColor = Color.FromArgb(230, 235, 240);
            int y = 10;

            panelMainMenu.Controls.Add(CreateButton("Поиск", new Point(0, y), btnColor, BtnSearch_Click));
            y += 50;
            panelMainMenu.Controls.Add(CreateButton("Фильтрация", new Point(0, y), btnColor, BtnFilter_Click));
            y += 50;
            panelMainMenu.Controls.Add(CreateButton("Сортировка", new Point(0, y), btnColor, BtnSort_Click));
            y += 50;

            if (currentUserRole == "Администратор")
            {
                panelMainMenu.Controls.Add(CreateButton("Добавить запись", new Point(0, y), btnColor, BtnAdd_Click));
                y += 50;
                panelMainMenu.Controls.Add(CreateButton("Редактировать запись", new Point(0, y), btnColor, BtnEdit_Click));
                y += 50;
                panelMainMenu.Controls.Add(CreateButton("Удалить запись", new Point(0, y), btnColor, BtnDelete_Click));
                y += 50;
            }

            panelMainMenu.Controls.Add(CreateButton("Создать отчёт (Word)", new Point(0, y), btnColor, BtnReport_Click));
            y += 60;

            panelMainMenu.Controls.Add(CreateButton("Выйти из приложения", new Point(0, y),
                Color.FromArgb(245, 220, 220), BtnExit_Click));
            y += 50;
            panelMainMenu.Controls.Add(CreateButton("Выйти из аккаунта", new Point(0, y),
                Color.FromArgb(240, 240, 240), BtnLogout_Click));
        }

        private void CreateSearchPanel()
        {
            panelSearch.Controls.Add(CreateLabel("Поле для поиска:", new Point(0, 5), true));
            ComboBox cmbColumn = CreateComboBox(new Point(0, 28));
            cmbColumn.Name = "cmbSearchColumn";
            panelSearch.Controls.Add(cmbColumn);

            panelSearch.Controls.Add(CreateLabel("Поисковый запрос:", new Point(0, 65), true));
            TextBox txtSearch = CreateTextBox(new Point(0, 88));
            txtSearch.Name = "txtSearchValue";
            panelSearch.Controls.Add(txtSearch);

            Button btnDoSearch = CreateButton("Найти", new Point(0, 125),
                Color.FromArgb(220, 240, 220), BtnDoSearch_Click);
            panelSearch.Controls.Add(btnDoSearch);

            Button btnShowAll = CreateButton("Показать все", new Point(0, 175),
                Color.FromArgb(230, 235, 240), BtnShowAll_Click);
            panelSearch.Controls.Add(btnShowAll);

            Button btnBack = CreateButton("Вернуться назад", new Point(0, 280),
                Color.FromArgb(240, 240, 240), BtnBackToMenu_Click);
            panelSearch.Controls.Add(btnBack);
        }

        private void CreateFilterPanel()
        {
            panelFilter.Controls.Add(CreateLabel("Поле для фильтрации:", new Point(0, 5), true));
            ComboBox cmbColumn = CreateComboBox(new Point(0, 28));
            cmbColumn.Name = "cmbFilterColumn";
            cmbColumn.SelectedIndexChanged += CmbFilterColumn_SelectedIndexChanged;
            panelFilter.Controls.Add(cmbColumn);

            panelFilter.Controls.Add(CreateLabel("Значение:", new Point(0, 65), true));
            ComboBox cmbValue = CreateComboBox(new Point(0, 88));
            cmbValue.Name = "cmbFilterValue";
            panelFilter.Controls.Add(cmbValue);

            Button btnApplyFilter = CreateButton("Применить фильтр", new Point(0, 125),
                Color.FromArgb(220, 240, 220), BtnApplyFilter_Click);
            panelFilter.Controls.Add(btnApplyFilter);

            Button btnShowAll = CreateButton("Сбросить фильтр", new Point(0, 175),
                Color.FromArgb(230, 235, 240), BtnShowAll_Click);
            panelFilter.Controls.Add(btnShowAll);

            Button btnBack = CreateButton("Вернуться назад", new Point(0, 280),
                Color.FromArgb(240, 240, 240), BtnBackToMenu_Click);
            panelFilter.Controls.Add(btnBack);
        }

        private void CreateSortPanel()
        {
            panelSort.Controls.Add(CreateLabel("Поле для сортировки:", new Point(0, 5), true));
            ComboBox cmbColumn = CreateComboBox(new Point(0, 28));
            cmbColumn.Name = "cmbSortColumn";
            panelSort.Controls.Add(cmbColumn);

            Button btnAsc = CreateButton("По возрастанию (А-Я / 0-9)", new Point(0, 70),
                Color.FromArgb(220, 240, 220), BtnSortAsc_Click);
            panelSort.Controls.Add(btnAsc);

            Button btnDesc = CreateButton("По убыванию (Я-А / 9-0)", new Point(0, 120),
                Color.FromArgb(220, 240, 220), BtnSortDesc_Click);
            panelSort.Controls.Add(btnDesc);

            Button btnShowAll = CreateButton("Сбросить сортировку", new Point(0, 170),
                Color.FromArgb(230, 235, 240), BtnShowAll_Click);
            panelSort.Controls.Add(btnShowAll);

            Button btnBack = CreateButton("Вернуться назад", new Point(0, 280),
                Color.FromArgb(240, 240, 240), BtnBackToMenu_Click);
            panelSort.Controls.Add(btnBack);
        }

        private void CreateAddPanel()
        {
            panelAdd.AutoScroll = true;
        }

        private void CreateEditPanel()
        {
            panelEdit.AutoScroll = true;
        }

        private void CreateDeletePanel()
        {
            panelDelete.Controls.Add(CreateLabel("Выберите запись в таблице справа", new Point(0, 5), true));
            panelDelete.Controls.Add(CreateLabel("и нажмите кнопку ниже для удаления.", new Point(0, 28), true));

            Button btnDelete = CreateButton("Удалить выбранную запись", new Point(0, 70),
                Color.FromArgb(245, 220, 220), BtnDoDelete_Click);
            panelDelete.Controls.Add(btnDelete);

            Button btnBack = CreateButton("Вернуться назад", new Point(0, 280),
                Color.FromArgb(240, 240, 240), BtnBackToMenu_Click);
            panelDelete.Controls.Add(btnBack);
        }

        private void CreateReportPanel()
        {
            panelReport.Controls.Add(CreateLabel("Формат отчёта:", new Point(0, 5), true));
            ComboBox cmbFormat = CreateComboBox(new Point(0, 28));
            cmbFormat.Name = "cmbReportFormat";
            cmbFormat.Items.Add("Word (.doc)");
            cmbFormat.Items.Add("Текстовый файл (.txt)");
            cmbFormat.SelectedIndex = 0;
            panelReport.Controls.Add(cmbFormat);

            panelReport.Controls.Add(CreateLabel("Отчёт будет содержать все данные", new Point(0, 70)));
            panelReport.Controls.Add(CreateLabel("текущей выбранной таблицы.", new Point(0, 93)));

            Button btnCreateReport = CreateButton("Сформировать отчёт", new Point(0, 130),
                Color.FromArgb(220, 240, 220), BtnCreateReport_Click);
            panelReport.Controls.Add(btnCreateReport);

            Button btnBack = CreateButton("Вернуться назад", new Point(0, 280),
                Color.FromArgb(240, 240, 240), BtnBackToMenu_Click);
            panelReport.Controls.Add(btnBack);
        }

        private void ShowPanel(Panel panelToShow)
        {
            foreach (Control ctrl in splitContainer1.Panel1.Controls)
            {
                if (ctrl is Panel p && p != panelToShow)
                {
                    p.Visible = false;
                    p.Controls.Clear();
                }
            }

            if (panelToShow == panelMainMenu) CreateMainMenuButtons();
            else if (panelToShow == panelSearch) CreateSearchPanel();
            else if (panelToShow == panelFilter) CreateFilterPanel();
            else if (panelToShow == panelSort) CreateSortPanel();
            else if (panelToShow == panelAdd) CreateAddPanel();
            else if (panelToShow == panelEdit) CreateEditPanel();
            else if (panelToShow == panelDelete) CreateDeletePanel();
            else if (panelToShow == panelReport) CreateReportPanel();

            panelToShow.Visible = true;

            PopulateColumnComboBoxes();
        }

        private void PopulateColumnComboBoxes()
        {
            if (string.IsNullOrEmpty(currentTableName)) return;

            try
            {
                List<string> columns = _dbHelper.GetColumnNames(currentTableName);

                foreach (Control ctrl in splitContainer1.Panel1.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        foreach (Control child in panel.Controls)
                        {
                            if (child is ComboBox cmb &&
                                (cmb.Name == "cmbSearchColumn" ||
                                 cmb.Name == "cmbFilterColumn" ||
                                 cmb.Name == "cmbSortColumn"))
                            {
                                cmb.Items.Clear();
                                foreach (string col in columns)
                                {
                                    cmb.Items.Add(col);
                                }
                                if (cmb.Items.Count > 0)
                                    cmb.SelectedIndex = 0;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void CmbFilterColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmbColumn = sender as ComboBox;
            if (cmbColumn == null || cmbColumn.SelectedItem == null) return;

            string columnName = cmbColumn.SelectedItem.ToString();
            ComboBox cmbValue = FindControlInPanel(panelFilter, "cmbFilterValue") as ComboBox;
            if (cmbValue == null) return;

            try
            {
                List<string> values = _dbHelper.GetDistinctValues(currentTableName, columnName);
                cmbValue.Items.Clear();
                cmbValue.Items.Add("(Все значения)");
                foreach (string val in values)
                {
                    cmbValue.Items.Add(val);
                }
                cmbValue.SelectedIndex = 0;
            }
            catch { }
        }

        private Control FindControlInPanel(Panel panel, string name)
        {
            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl.Name == name) return ctrl;
            }
            return null;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            if (!CheckTableSelected()) return;
            ShowPanel(panelSearch);
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            if (!CheckTableSelected()) return;
            ShowPanel(panelFilter);
        }

        private void BtnSort_Click(object sender, EventArgs e)
        {
            if (!CheckTableSelected()) return;
            ShowPanel(panelSort);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!CheckTableSelected()) return;
            ShowPanel(panelAdd);
            CreateDynamicForm(panelAdd, null, true);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (!CheckTableSelected()) return;
            if (dataGridViewMain.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ShowPanel(panelEdit);
            CreateDynamicForm(panelEdit, dataGridViewMain.CurrentRow, false);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!CheckTableSelected()) return;
            ShowPanel(panelDelete);
        }

        private void BtnReport_Click(object sender, EventArgs e)
        {
            if (!CheckTableSelected()) return;
            ShowPanel(panelReport);
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnBackToMenu_Click(object sender, EventArgs e)
        {
            ShowPanel(panelMainMenu);
        }

        private void BtnDoSearch_Click(object sender, EventArgs e)
        {
            ComboBox cmbColumn = FindControlInPanel(panelSearch, "cmbSearchColumn") as ComboBox;
            TextBox txtValue = FindControlInPanel(panelSearch, "txtSearchValue") as TextBox;

            if (cmbColumn == null || txtValue == null) return;
            if (string.IsNullOrWhiteSpace(txtValue.Text))
            {
                MessageBox.Show("Введите поисковый запрос!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                currentData = _dbHelper.SearchData(currentTableName, cmbColumn.SelectedItem.ToString(), txtValue.Text.Trim());
                dataGridViewMain.DataSource = currentData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnApplyFilter_Click(object sender, EventArgs e)
        {
            ComboBox cmbColumn = FindControlInPanel(panelFilter, "cmbFilterColumn") as ComboBox;
            ComboBox cmbValue = FindControlInPanel(panelFilter, "cmbFilterValue") as ComboBox;

            if (cmbColumn == null || cmbValue == null) return;
            if (cmbValue.SelectedIndex <= 0)
            {
                MessageBox.Show("Выберите значение для фильтрации!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                currentData = _dbHelper.FilterData(currentTableName, cmbColumn.SelectedItem.ToString(), cmbValue.SelectedItem.ToString());
                dataGridViewMain.DataSource = currentData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSortAsc_Click(object sender, EventArgs e)
        {
            ComboBox cmbColumn = FindControlInPanel(panelSort, "cmbSortColumn") as ComboBox;
            if (cmbColumn == null || cmbColumn.SelectedItem == null)
            {
                MessageBox.Show("Выберите поле для сортировки!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                currentData = _dbHelper.SortData(currentTableName, cmbColumn.SelectedItem.ToString(), true);
                dataGridViewMain.DataSource = currentData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сортировки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSortDesc_Click(object sender, EventArgs e)
        {
            ComboBox cmbColumn = FindControlInPanel(panelSort, "cmbSortColumn") as ComboBox;
            if (cmbColumn == null || cmbColumn.SelectedItem == null)
            {
                MessageBox.Show("Выберите поле для сортировки!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                currentData = _dbHelper.SortData(currentTableName, cmbColumn.SelectedItem.ToString(), false);
                dataGridViewMain.DataSource = currentData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сортировки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            LoadTableData();
        }

        // Определяем AutoNumber поля (ID, Регистрационный_номер и т.д.)
        // Жёстко задаём AutoNumber поля для каждой таблицы
        private List<string> GetAutoNumberColumns()
        {
            var autoNumberColumns = new List<string>();

            switch (currentTableName)
            {
                case "Пользователи":
                    autoNumberColumns.Add("ID");
                    break;
                case "Абитуриент":
                    autoNumberColumns.Add("Регистрационный_номер");
                    break;
                //case "Что_окончил":
                //    autoNumberColumns.Add("Код_учебного_заведения");
                //    break;
                case "Факультет":
                    autoNumberColumns.Add("Код_факультета");
                    break;
                case "Специальность":
                    autoNumberColumns.Add("Код_специальности");
                    break;
                case "Специализация":
                    autoNumberColumns.Add("Код_специализации");
                    break;
            }

            return autoNumberColumns;
        }

        // Определяем поля с вариантами Да/Нет
        private List<string> GetYesNoFields()
        {
            var yesNoFields = new List<string>();

            if (currentTableName == "Абитуриент")
            {
                yesNoFields.Add("Наличие_медали");
            }

            return yesNoFields;
        }

        private Dictionary<string, string> GetForeignKeyMappings()
        {
            var fkMappings = new Dictionary<string, string>();

            // Жёстко задаём связи на основе схемы БД
            if (currentTableName == "Абитуриент")
            {
                fkMappings["Код_учебного_заведения"] = "Что_окончил";
                fkMappings["Код_специализации"] = "Специализация";
            }
            else if (currentTableName == "Специализация")
            {
                fkMappings["Код_специальности"] = "Специальность";
            }
            else if (currentTableName == "Специальность")
            {
                fkMappings["Код_факультета"] = "Факультет";
            }

            return fkMappings;
        }


        private void CreateDynamicForm(Panel panel, DataGridViewRow row, bool isAdd)
        {
            panel.Controls.Clear();

            List<string> columns = _dbHelper.GetColumnNames(currentTableName);
            var autoNumberColumns = GetAutoNumberColumns();
            var fkMappings = GetForeignKeyMappings();
            var yesNoFields = GetYesNoFields();

            int y = 5;

            if (isAdd)
            {
                panel.Controls.Add(CreateLabel("Добавление новой записи", new Point(0, y), true));
                y += 25;
            }
            else
            {
                panel.Controls.Add(CreateLabel("Редактирование записи", new Point(0, y), true));
                y += 25;
            }

            Dictionary<string, TextBox> textBoxes = new Dictionary<string, TextBox>();
            Dictionary<string, ComboBox> comboBoxes = new Dictionary<string, ComboBox>();

            foreach (string col in columns)
            {
                bool isAutoNumber = autoNumberColumns.Contains(col);
                bool isForeignKey = fkMappings.ContainsKey(col);
                bool isYesNo = yesNoFields.Contains(col);

                panel.Controls.Add(CreateLabel(col + ":", new Point(0, y)));
                y += 20;

                if (isAutoNumber)
                {
                    // AutoNumber - только для чтения
                    TextBox txt = CreateTextBox(new Point(0, y));
                    txt.Name = "txt_" + col;
                    txt.ReadOnly = true;
                    txt.BackColor = Color.FromArgb(240, 240, 240);

                    if (!isAdd && row != null)
                    {
                        txt.Text = row.Cells[col].Value?.ToString() ?? "";
                    }
                    else
                    {
                        txt.Text = "(автоматически)";
                    }

                    textBoxes[col] = txt;
                    panel.Controls.Add(txt);
                }
                else if (isYesNo)
                {
                    // Поле Да/Нет - ComboBox с двумя вариантами
                    ComboBox cmb = CreateComboBox(new Point(0, y));
                    cmb.Name = "cmb_" + col;
                    cmb.Items.Add("да");
                    cmb.Items.Add("нет");

                    // Устанавливаем текущее значение при редактировании
                    if (!isAdd && row != null && row.Cells[col].Value != null)
                    {
                        string currentValue = row.Cells[col].Value.ToString().ToLower();
                        if (currentValue == "да" || currentValue == "нет")
                        {
                            cmb.SelectedItem = currentValue;
                        }
                        else
                        {
                            cmb.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        cmb.SelectedIndex = 0;
                    }

                    comboBoxes[col] = cmb;
                    panel.Controls.Add(cmb);
                }
                else if (isForeignKey)
                {
                    // Foreign Key - ComboBox со значениями из связанной таблицы
                    ComboBox cmb = CreateComboBox(new Point(0, y));
                    cmb.Name = "cmb_" + col;

                    try
                    {
                        string relatedTable = fkMappings[col];
                        var relatedData = _dbHelper.GetAllData(relatedTable);

                        if (relatedData.Columns.Count >= 2)
                        {
                            string codeColumn = relatedData.Columns[0].ColumnName;
                            string nameColumn = relatedData.Columns[1].ColumnName;

                            cmb.Items.Add("(не выбрано)");

                            foreach (DataRow relatedRow in relatedData.Rows)
                            {
                                string code = relatedRow[codeColumn].ToString();
                                string name = relatedRow[nameColumn].ToString();
                                string display = $"{code} - {name}";

                                cmb.Items.Add(display);
                            }

                            if (!isAdd && row != null && row.Cells[col].Value != null)
                            {
                                string currentValue = row.Cells[col].Value.ToString();
                                foreach (string item in cmb.Items)
                                {
                                    if (item.StartsWith(currentValue + " -"))
                                    {
                                        cmb.SelectedItem = item;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                cmb.SelectedIndex = 0;
                            }
                        }
                        else
                        {
                            TextBox txt = CreateTextBox(new Point(0, y));
                            txt.Name = "txt_" + col;
                            txt.Text = (!isAdd && row != null) ? row.Cells[col].Value?.ToString() ?? "" : "";
                            textBoxes[col] = txt;
                            panel.Controls.Add(txt);
                            y += 35;
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        TextBox txt = CreateTextBox(new Point(0, y));
                        txt.Name = "txt_" + col;
                        txt.Text = (!isAdd && row != null) ? row.Cells[col].Value?.ToString() ?? "" : "";
                        textBoxes[col] = txt;
                        panel.Controls.Add(txt);
                        y += 35;
                        continue;
                    }

                    comboBoxes[col] = cmb;
                    panel.Controls.Add(cmb);
                }
                else
                {
                    // Обычное поле - TextBox
                    TextBox txt = CreateTextBox(new Point(0, y));
                    txt.Name = "txt_" + col;

                    if (!isAdd && row != null)
                    {
                        txt.Text = row.Cells[col].Value?.ToString() ?? "";
                    }

                    textBoxes[col] = txt;
                    panel.Controls.Add(txt);
                }

                y += 35;
            }

            Button btnSubmit = CreateButton(isAdd ? "Добавить" : "Сохранить", new Point(0, y + 10),
                Color.FromArgb(220, 240, 220), (s, ev) => BtnSubmitDynamic_Click(s, ev, textBoxes, comboBoxes, isAdd, row));
            panel.Controls.Add(btnSubmit);

            Button btnBack = CreateButton("Вернуться назад", new Point(0, y + 60),
                Color.FromArgb(240, 240, 240), BtnBackToMenu_Click);
            panel.Controls.Add(btnBack);
        }

        private void BtnSubmitDynamic_Click(object sender, EventArgs e,
     Dictionary<string, TextBox> textBoxes,
     Dictionary<string, ComboBox> comboBoxes,
     bool isAdd, DataGridViewRow row)
        {
            try
            {
                Dictionary<string, object> values = new Dictionary<string, object>();
                var autoNumberColumns = GetAutoNumberColumns();
                var yesNoFields = GetYesNoFields();

                // Обрабатываем TextBox поля
                foreach (var kvp in textBoxes)
                {
                    string fieldName = kvp.Key;
                    string val = kvp.Value.Text.Trim();

                    if (isAdd && autoNumberColumns.Contains(fieldName))
                    {
                        continue;
                    }

                    values[fieldName] = string.IsNullOrEmpty(val) ? DBNull.Value : (object)val;
                }

                // Обрабатываем ComboBox поля
                foreach (var kvp in comboBoxes)
                {
                    string fieldName = kvp.Key;
                    ComboBox cmb = kvp.Value;

                    if (yesNoFields.Contains(fieldName))
                    {
                        // Для полей Да/Нет сохраняем выбранное значение как есть
                        if (cmb.SelectedItem != null)
                        {
                            values[fieldName] = cmb.SelectedItem.ToString();
                        }
                        else
                        {
                            values[fieldName] = "нет"; // по умолчанию
                        }
                    }
                    else if (cmb.SelectedIndex <= 0) // "(не выбрано)" для FK
                    {
                        values[fieldName] = DBNull.Value;
                    }
                    else
                    {
                        // Для FK извлекаем код из строки "1 - Название"
                        string selectedText = cmb.SelectedItem.ToString();
                        string code = selectedText.Split(new[] { " - " }, StringSplitOptions.None)[0];
                        values[fieldName] = code;
                    }
                }

                if (isAdd)
                {
                    _dbHelper.InsertRecord(currentTableName, values);
                    MessageBox.Show("Запись успешно добавлена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    object keyValue = row.Cells[primaryKeyColumn].Value;
                    values.Remove(primaryKeyColumn);
                    _dbHelper.UpdateRecord(currentTableName, values, primaryKeyColumn, keyValue);
                    MessageBox.Show("Запись успешно обновлена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadTableData();
                ShowPanel(panelMainMenu);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDoDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewMain.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для удаления!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    object keyValue = dataGridViewMain.CurrentRow.Cells[primaryKeyColumn].Value;
                    _dbHelper.DeleteRecord(currentTableName, primaryKeyColumn, keyValue);
                    MessageBox.Show("Запись удалена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTableData();
                    ShowPanel(panelMainMenu);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnCreateReport_Click(object sender, EventArgs e)
        {
            ComboBox cmbFormat = FindControlInPanel(panelReport, "cmbReportFormat") as ComboBox;
            string format = cmbFormat?.SelectedItem?.ToString() ?? "Word (.doc)";

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                if (format.Contains("Word"))
                {
                    saveDialog.Filter = "Word Document|*.doc";
                    saveDialog.DefaultExt = ".doc";
                }
                else
                {
                    saveDialog.Filter = "Text File|*.txt";
                    saveDialog.DefaultExt = ".txt";
                }
                saveDialog.FileName = $"Отчёт_{currentTableName}_{DateTime.Now:yyyyMMdd}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (format.Contains("Word"))
                    {
                        CreateWordReport(saveDialog.FileName);
                    }
                    else
                    {
                        CreateTextReport(saveDialog.FileName);
                    }
                    MessageBox.Show("Отчёт успешно создан!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания отчёта: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateWordReport(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='utf-8'></head><body>");
            sb.AppendLine($"<h1>Отчёт по таблице: {currentTableName}</h1>");
            sb.AppendLine($"<p>Дата формирования: {DateTime.Now}</p>");
            sb.AppendLine($"<p>Пользователь: {currentUserRole}</p>");
            sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0'>");

            sb.AppendLine("<tr>");
            foreach (DataColumn col in currentData.Columns)
            {
                sb.AppendLine($"<th><b>{col.ColumnName}</b></th>");
            }
            sb.AppendLine("</tr>");

            foreach (DataRow row in currentData.Rows)
            {
                sb.AppendLine("<tr>");
                foreach (DataColumn col in currentData.Columns)
                {
                    sb.AppendLine($"<td>{row[col]}</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine($"<p>Всего записей: {currentData.Rows.Count}</p>");
            sb.AppendLine("</body></html>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private void CreateTextReport(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"ОТЧЁТ ПО ТАБЛИЦЕ: {currentTableName}");
            sb.AppendLine($"Дата формирования: {DateTime.Now}");
            sb.AppendLine($"Пользователь: {currentUserRole}");
            sb.AppendLine(new string('-', 80));

            string header = "";
            foreach (DataColumn col in currentData.Columns)
            {
                header += col.ColumnName.PadRight(20);
            }
            sb.AppendLine(header);
            sb.AppendLine(new string('-', 80));

            foreach (DataRow row in currentData.Rows)
            {
                string line = "";
                foreach (DataColumn col in currentData.Columns)
                {
                    line += row[col].ToString().PadRight(20);
                }
                sb.AppendLine(line);
            }

            sb.AppendLine(new string('-', 80));
            sb.AppendLine($"Всего записей: {currentData.Rows.Count}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private bool CheckTableSelected()
        {
            if (string.IsNullOrEmpty(currentTableName))
            {
                MessageBox.Show("Сначала выберите таблицу!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}