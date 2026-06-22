using Exceptions;
using Logic;
using PractiStudent.Data;
using StudentSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PractiStudent
{
    public partial class FormMain : Form
    {
        private readonly string _currentUserRole;
        private readonly string _databaseFileName;
        private readonly UserService _userService;
        private readonly DatabaseHelper _dbHelper;
        private readonly TableOperations _tableOps;

        private string _currentTableName = "";
        private DataTable _currentData;
        private string _primaryKeyColumn = "";
                
        private ComboBox cmbTables; // интерфейса элементы
        private Label lblTableSelect;
        private Label lblConnectionInfo;
                
        private Panel panelMainMenu; // панели действий
        private Panel panelSearch;
        private Panel panelFilter;
        private Panel panelSort;
        private Panel panelAdd;
        private Panel panelEdit;
        private Panel panelDelete;
        private Panel panelReport;                
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x200; // убрать крестик
                return cp;
            }
        }
        public FormMain(string role, int? regNumber, string dbFileName, UserService userService)
        {
            InitializeComponent();
            _currentUserRole = role;
            _databaseFileName = dbFileName;
            _userService = userService;
            _dbHelper = new DatabaseHelper();
            _dbHelper.SetDatabaseConnection(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName));
            _tableOps = new TableOperations(_dbHelper);

            InitializeForm();
            LoadTablesIntoComboBox();
        }
        private void InitializeForm()
        {
            ConfigureForm();
            InitializeDataGridView();
            InitializeConnectionIndicator();
            InitializeLeftPanel();
            CreateAllActionPanels();
            ShowPanel(FormModes.MainMenu);
        }        
        private void ConfigureForm() // настройка внешнего вида формы
        {
            this.Text = _currentUserRole == TableConstants.RoleAdmin
                ? UIStyles.MainFormTitleAdmin
                : UIStyles.MainFormTitleGuest;
            this.Size = UIStyles.MainFormSize;
            this.StartPosition = UIStyles.DefaultFormStartPosition;
            this.FormBorderStyle = UIStyles.DefaultBorderStyle;
            this.MaximizeBox = UIStyles.AllowMaximize;
            this.MinimizeBox = UIStyles.AllowMinimize;
            this.BackColor = UIStyles.Background;
        }        
        private void InitializeDataGridView() // настройка DataGridView
        {
            dataGridViewMain.ScrollBars = ScrollBars.Both;
            dataGridViewMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewMain.ReadOnly = true;
            dataGridViewMain.AllowUserToAddRows = false;
            dataGridViewMain.AllowUserToDeleteRows = false;
            dataGridViewMain.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewMain.MultiSelect = false;
            dataGridViewMain.BackgroundColor = UIStyles.DataGridViewRowBackColor;
            dataGridViewMain.RowHeadersVisible = false;

            dataGridViewMain.ColumnHeadersDefaultCellStyle.BackColor = UIStyles.DataGridViewHeaderBackColor;
            dataGridViewMain.ColumnHeadersDefaultCellStyle.ForeColor = UIStyles.TextColor;
            dataGridViewMain.ColumnHeadersDefaultCellStyle.Font = UIStyles.DataGridViewHeaderFont;
            dataGridViewMain.DefaultCellStyle.BackColor = UIStyles.DataGridViewRowBackColor;
            dataGridViewMain.DefaultCellStyle.ForeColor = UIStyles.TextColor;
            dataGridViewMain.DefaultCellStyle.Font = UIStyles.DataGridViewFont;
            dataGridViewMain.AlternatingRowsDefaultCellStyle.BackColor = UIStyles.DataGridViewAltRowBackColor;
            dataGridViewMain.GridColor = UIStyles.DataGridViewGridColor;
            dataGridViewMain.BorderStyle = BorderStyle.None;
            dataGridViewMain.EnableHeadersVisualStyles = false;
        }        
        private void InitializeConnectionIndicator() // создание индикатора подключения
        {
            lblConnectionInfo = new Label
            {
                Text = $"{UIStyles.DatabaseConnectionPrefix}{_databaseFileName}",
                Font = UIStyles.HintFont,
                ForeColor = UIStyles.TextColor,
                Location = UIStyles.ConnectionInfoPosition,
                Size = UIStyles.ConnectionInfoSize,
                TextAlign = ContentAlignment.MiddleLeft
            };
            splitContainer1.Panel1.Controls.Add(lblConnectionInfo);
            lblConnectionInfo.BringToFront();
        }        
        private void InitializeLeftPanel() // инициализация левой панели с выбором таблицы
        {
            lblTableSelect = PanelBuilder.CreateLabel("Выберите таблицу:", UIStyles.TableSelectLabelPosition, true);
            cmbTables = PanelBuilder.CreateComboBox(UIStyles.TableSelectComboPosition);
            cmbTables.Size = UIStyles.TableSelectComboSize;
            cmbTables.SelectedIndexChanged += CmbTables_SelectedIndexChanged;

            splitContainer1.Panel1.Controls.Add(lblTableSelect);
            splitContainer1.Panel1.Controls.Add(cmbTables);
        }        
        private void LoadTablesIntoComboBox() // загрузка списка таблиц в ComboBox
        {
            try
            {
                List<string> tables = _dbHelper.GetTableNames();
                cmbTables.Items.Clear();
                foreach (string table in tables)
                {
                    if (ShouldAddTableToComboBox(table))
                    {
                        cmbTables.Items.Add(table);
                    }
                }
                SelectFirstTableOrDefault();
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "LoadTables");
            }
        }        
        private bool ShouldAddTableToComboBox(string table) // проверка, нужно ли добавлять таблицу в список
        {
            if (table.Equals(TableConstants.TableUsers, StringComparison.OrdinalIgnoreCase))
            {
                return _currentUserRole == TableConstants.RoleAdmin;
            }
            return true;
        }        
        private void SelectFirstTableOrDefault() // выбор первой таблицы или показ сообщения
        {
            if (cmbTables.Items.Count > 0)
            {
                cmbTables.SelectedIndex = 0;
            }
            else
            {
                ErrorHandler.ShowInfo("Нет доступных таблиц для просмотра.");
            }
        }        
        private void CmbTables_SelectedIndexChanged(object sender, EventArgs e) // обработчик изменения выбранной таблицы
        {
            if (cmbTables.SelectedItem != null)
            {
                _currentTableName = cmbTables.SelectedItem.ToString();
                ClearActionPanels();
                LoadTableData();
                ShowPanel(FormModes.MainMenu);
            }
        }        
        private void LoadTableData() // загрузка данных выбранной таблицы
        {
            if (string.IsNullOrEmpty(_currentTableName)) 
                return;
            try
            {
                _currentData = _dbHelper.GetAllData(_currentTableName);
                DataFormatter.FormatDatesInTable(_currentData);
                dataGridViewMain.DataSource = _currentData;
                HidePasswordColumnIfNeeded();
                SetPrimaryKeyColumn();
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "LoadTableData");
            }
        }        
        private void HidePasswordColumnIfNeeded() // скрытие столбца с хэшем пароля для таблицы пользователей
        {
            if (_currentTableName == TableConstants.TableUsers)
            {
                dataGridViewMain.Columns[TableConstants.FieldPasswordHash].Visible = false;
            }
        }        
        private void SetPrimaryKeyColumn() // установка имени первичного ключа
        {
            if (_currentData.Columns.Count > 0)
            {
                _primaryKeyColumn = _currentData.Columns[0].ColumnName;
            }
        }        
        private void CreateAllActionPanels() // создание всех панелей действий
        {
            panelMainMenu = CreatePanel(FormModes.MainMenu);
            CreateMainMenuButtons();
            panelSearch = CreatePanel(FormModes.Search);
            CreateSearchPanel();
            panelFilter = CreatePanel(FormModes.Filter);
            CreateFilterPanel();
            panelSort = CreatePanel(FormModes.Sort);
            CreateSortPanel();
            if (_currentUserRole == TableConstants.RoleAdmin)
            {
                panelAdd = CreatePanel(FormModes.Add);
                panelEdit = CreatePanel(FormModes.Edit);
                panelDelete = CreatePanel(FormModes.Delete);
            }
            panelReport = CreatePanel(FormModes.Report);
            CreateReportPanel();
        }
        private Panel CreatePanel(string mode) // создание панели
        {
            Panel panel = PanelBuilder.CreateActionPanel(UIStyles.ActionPanelTop, 700); 
            splitContainer1.Panel1.Controls.Add(panel);
            return panel;
        }
        private void ShowPanel(string mode) // показ выбранной панели
        {
            HideAllPanels();
            RebuildPanel(mode);
            GetPanelByMode(mode).Visible = true;
            PopulateColumnComboBoxes();
        }        
        private void HideAllPanels() // скрытие всех панелей
        {
            foreach (Control ctrl in splitContainer1.Panel1.Controls)
            {
                if (ctrl is Panel p)
                {
                    p.Visible = false;
                    p.Controls.Clear();
                }
            }
        }        
        private Panel GetPanelByMode(string mode) // получение панели по режиму
        {
            return mode switch
            {
                FormModes.MainMenu => panelMainMenu,
                FormModes.Search => panelSearch,
                FormModes.Filter => panelFilter,
                FormModes.Sort => panelSort,
                FormModes.Add => panelAdd,
                FormModes.Edit => panelEdit,
                FormModes.Delete => panelDelete,
                FormModes.Report => panelReport,
                _ => panelMainMenu
            };
        }        
        private void RebuildPanel(string mode) // перестроение содержимого панели
        {
            switch (mode)
            {
                case FormModes.MainMenu:
                    CreateMainMenuButtons();
                    break;
                case FormModes.Search:
                    CreateSearchPanel();
                    break;
                case FormModes.Filter:
                    CreateFilterPanel();
                    break;
                case FormModes.Sort:
                    CreateSortPanel();
                    break;
                case FormModes.Add:
                    CreateDynamicForm(panelAdd, null, true);
                    break;
                case FormModes.Edit:
                    if (dataGridViewMain.CurrentRow != null)
                    {
                        CreateDynamicForm(panelEdit, dataGridViewMain.CurrentRow, false);
                    }
                    break;
                case FormModes.Delete:
                    CreateDeletePanel();
                    break;
                case FormModes.Report:
                    CreateReportPanel();
                    break;
            }
        }       
        private void ClearActionPanels()  // очистка панелей 
        {
            panelAdd?.Controls.Clear();
            panelEdit?.Controls.Clear();
            panelDelete?.Controls.Clear();
        }        
        private void CreateMainMenuButtons() // создание кнопок главного меню
        {
            panelMainMenu.Controls.Clear();
            int y = UIStyles.ActionButtonPosition.Y;

            AddMainMenuButton("Поиск", ref y, UIStyles.PrimaryButton, BtnSearch_Click);
            AddMainMenuButton("Фильтрация", ref y, UIStyles.PrimaryButton, BtnFilter_Click);
            AddMainMenuButton("Сортировка", ref y, UIStyles.PrimaryButton, BtnSort_Click);

            if (_currentUserRole == TableConstants.RoleAdmin)
            {
                AddMainMenuButton("Добавить запись", ref y, UIStyles.PrimaryButton, BtnAdd_Click);
                AddMainMenuButton("Редактировать запись", ref y, UIStyles.PrimaryButton, BtnEdit_Click);
                AddMainMenuButton("Удалить запись", ref y, UIStyles.PrimaryButton, BtnDelete_Click);
            }

            AddMainMenuButton("Создать отчёт (Word)", ref y, UIStyles.PrimaryButton, BtnReport_Click);
            y += 60;
            AddMainMenuButton("Выйти из приложения", ref y, UIStyles.DangerButton, BtnExit_Click);
            AddMainMenuButton("Выйти из аккаунта", ref y, UIStyles.NeutralButton, BtnLogout_Click);
        }        
        private void AddMainMenuButton(string text, ref int y, Color backColor, EventHandler click) // добавление кнопки в главное меню
        {
            panelMainMenu.Controls.Add(PanelBuilder.CreateButton(text, new Point(0, y), backColor, click));
            y += UIStyles.ButtonVerticalSpacing;
        }       
        private void CreateSearchPanel() // создание панели поиска
        {
            panelSearch.Controls.Clear();
            int y = 5;

            AddLabelToPanel(panelSearch, "Поле для поиска:", ref y, true);
            AddComboBoxToPanel(panelSearch, "cmbSearchColumn", ref y);

            AddLabelToPanel(panelSearch, "Поисковый запрос:", ref y, true);
            AddTextBoxToPanel(panelSearch, "txtSearchValue", ref y);

            AddButtonToPanel(panelSearch, "Найти", ref y, UIStyles.SuccessButton, BtnDoSearch_Click);
            AddButtonToPanel(panelSearch, "Показать все", ref y, UIStyles.PrimaryButton, BtnShowAll_Click);
            AddButtonToPanel(panelSearch, "Вернуться назад", ref y, UIStyles.NeutralButton, BtnBackToMenu_Click);
        }        
        private void CreateFilterPanel()// создание панели фильтрации
        {
            panelFilter.Controls.Clear();
            int y = 5;

            AddLabelToPanel(panelFilter, "Поле для фильтрации:", ref y, true);
            ComboBox cmbColumn = AddComboBoxToPanel(panelFilter, "cmbFilterColumn", ref y);
            cmbColumn.SelectedIndexChanged += CmbFilterColumn_SelectedIndexChanged;

            AddLabelToPanel(panelFilter, "Значение:", ref y, true);
            ComboBox cmbValue = AddComboBoxToPanel(panelFilter, "cmbFilterValue", ref y);
            cmbValue.Items.Add("(все значения)");

            AddButtonToPanel(panelFilter, "Применить фильтр", ref y, UIStyles.SuccessButton, BtnApplyFilter_Click);
            AddButtonToPanel(panelFilter, "Сбросить фильтр", ref y, UIStyles.PrimaryButton, BtnShowAll_Click);
            AddButtonToPanel(panelFilter, "Вернуться назад", ref y, UIStyles.NeutralButton, BtnBackToMenu_Click);
        }        
        private void CreateSortPanel() // создание панели сортировки
        {
            panelSort.Controls.Clear();
            int y = 5;

            AddLabelToPanel(panelSort, "Поле для сортировки:", ref y, true);
            AddComboBoxToPanel(panelSort, "cmbSortColumn", ref y);

            AddButtonToPanel(panelSort, "По возрастанию", ref y, UIStyles.SuccessButton, BtnSortAsc_Click);
            AddButtonToPanel(panelSort, "По убыванию", ref y, UIStyles.SuccessButton, BtnSortDesc_Click);
            AddButtonToPanel(panelSort, "Сбросить сортировку", ref y, UIStyles.PrimaryButton, BtnShowAll_Click);
            AddButtonToPanel(panelSort, "Вернуться назад", ref y, UIStyles.NeutralButton, BtnBackToMenu_Click);
        }        
        private void CreateDeletePanel() // создание панели удаления
        {
            panelDelete.Controls.Clear();
            int y = 5;

            AddLabelToPanel(panelDelete, "Выберите запись в таблице справа", ref y, true);
            AddLabelToPanel(panelDelete, "и нажмите кнопку ниже для удаления.", ref y, false);
            AddButtonToPanel(panelDelete, "Удалить выбранную запись", ref y, UIStyles.DangerButton, BtnDoDelete_Click);
            AddButtonToPanel(panelDelete, "Вернуться назад", ref y, UIStyles.NeutralButton, BtnBackToMenu_Click);
        }        
        private void CreateReportPanel() // создание панели отчётов
        {
            panelReport.Controls.Clear();
            int y = 5;

            AddLabelToPanel(panelReport, "Формат отчёта:", ref y, true);
            ComboBox cmbFormat = AddComboBoxToPanel(panelReport, "cmbReportFormat", ref y);
            cmbFormat.Items.Add("Word (.doc)");
            cmbFormat.Items.Add("Текстовый файл (.txt)");
            cmbFormat.SelectedIndex = 0;

            AddLabelToPanel(panelReport, "Отчёт будет содержать все данные", ref y, false);
            AddLabelToPanel(panelReport, "текущей выбранной таблицы.", ref y, false);
            AddButtonToPanel(panelReport, "Сформировать отчёт", ref y, UIStyles.SuccessButton, BtnCreateReport_Click);
            AddButtonToPanel(panelReport, "Вернуться назад", ref y, UIStyles.NeutralButton, BtnBackToMenu_Click);
        }        
        private void AddLabelToPanel(Panel panel, string text, ref int y, bool bold) // добавление метки на панель
        {
            panel.Controls.Add(PanelBuilder.CreateLabel(text, new Point(0, y), bold));
            y += 23;
        }        
        private ComboBox AddComboBoxToPanel(Panel panel, string name, ref int y) // добавление ComboBox на панель
        {
            ComboBox cmb = PanelBuilder.CreateComboBox(new Point(0, y));
            cmb.Name = name;
            panel.Controls.Add(cmb);
            y += 37;
            return cmb;
        }        
        private TextBox AddTextBoxToPanel(Panel panel, string name, ref int y) // добавление TextBox на панель
        {
            TextBox txt = PanelBuilder.CreateTextBox(new Point(0, y));
            txt.Name = name;
            panel.Controls.Add(txt);
            y += 37;
            return txt;
        }        
        private void AddButtonToPanel(Panel panel, string text, ref int y, Color backColor, EventHandler click) // добавление кнопки на панель
        {
            panel.Controls.Add(PanelBuilder.CreateButton(text, new Point(0, y), backColor, click));
            y += 50;
        }       
        private void PopulateColumnComboBoxes() // заполнение ComboBox названиями столбцов
        {
            if (string.IsNullOrEmpty(_currentTableName)) 
                return;
            try
            {
                List<string> columns = _dbHelper.GetColumnNames(_currentTableName);

                foreach (Control ctrl in splitContainer1.Panel1.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        PopulateComboBoxesInPanel(panel, columns);
                    }
                }
            }
            catch { }
        }        
        private void PopulateComboBoxesInPanel(Panel panel, List<string> columns) // заполнение ComboBox в конкретной панели
        {
            foreach (Control child in panel.Controls)
            {
                if (child is ComboBox cmb && IsColumnComboBox(cmb))
                {
                    FillComboBoxWithColumns(cmb, columns);
                }
            }
        }        
        private bool IsColumnComboBox(ComboBox cmb) // проверка, является ли ComboBox списком столбцов
        {
            return cmb.Name == "cmbSearchColumn" ||
                   cmb.Name == "cmbFilterColumn" ||
                   cmb.Name == "cmbSortColumn";
        }        
        private void FillComboBoxWithColumns(ComboBox cmb, List<string> columns)// заполнение ComboBox названиями столбцов
        {
            cmb.Items.Clear();
            cmb.Items.AddRange(columns.ToArray());
            if (cmb.Items.Count > 0)
                cmb.SelectedIndex = 0;
        }        
        private void CmbFilterColumn_SelectedIndexChanged(object sender, EventArgs e) // загрузка уникальных значений для фильтра
        {
            ComboBox cmbColumn = sender as ComboBox;
            if (cmbColumn?.SelectedItem == null) 
                return;
            ComboBox cmbValue = FindControlInPanel(panelFilter, "cmbFilterValue") as ComboBox;
            if (cmbValue == null) 
                return;
            try
            {
                LoadDistinctValues(cmbColumn.SelectedItem.ToString(), cmbValue);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка загрузки значений для фильтра: {ex.Message}", ex, "CmbFilterColumn_SelectedIndexChanged");
            }
        }        
        private void LoadDistinctValues(string columnName, ComboBox cmbValue) // загрузка уникальных значений в ComboBox
        {
            List<string> values = _dbHelper.GetDistinctValues(_currentTableName, columnName);

            cmbValue.Items.Clear();
            cmbValue.Items.Add("(все значения)");

            foreach (string val in values)
            {
                if (!string.IsNullOrEmpty(val))
                {
                    cmbValue.Items.Add(DataFormatter.FormatValueForDisplay(val));
                }
            }

            cmbValue.SelectedIndex = 0;
        }        
        private Control FindControlInPanel(Panel panel, string name) // поиск элемента управления в панели
        {
            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl.Name == name) return ctrl;
            }
            return null;
        }        
        private void BtnSearch_Click(object sender, EventArgs e) // обработчики кнопок главного меню
        {
            if (CheckTableSelected()) 
                ShowPanel(FormModes.Search);
        }
        private void BtnFilter_Click(object sender, EventArgs e)
        {
            if (CheckTableSelected()) 
                ShowPanel(FormModes.Filter);
        }
        private void BtnSort_Click(object sender, EventArgs e)
        {
            if (CheckTableSelected()) 
                ShowPanel(FormModes.Sort);
        }
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!CheckTableSelected()) 
                return;
            if (_currentTableName == TableConstants.TableUsers)
            {
                ShowUserAdditionForbiddenMessage();
                return;
            }
            ShowPanel(FormModes.Add);
        }        
        private void ShowUserAdditionForbiddenMessage() // показ сообщения о запрете добавления пользователей
        {
            ErrorHandler.ShowWarning(
                "Добавление новых пользователей запрещено!\n\n" +
                "Пользователи создаются автоматически при регистрации через форму входа.");
        }
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (!CheckTableSelected()) return;
            if (dataGridViewMain.CurrentRow == null)
            {
                ErrorHandler.ShowWarning("Выберите запись для редактирования!");
                return;
            }
            ShowPanel(FormModes.Edit);
        }
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (CheckTableSelected()) 
                ShowPanel(FormModes.Delete);
        }
        private void BtnReport_Click(object sender, EventArgs e)
        {
            if (CheckTableSelected()) 
                ShowPanel(FormModes.Report);
        }
        private void BtnExit_Click(object sender, EventArgs e)
        {
            ForceCleanupComResources();
            Application.Exit();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            ForceCleanupComResources();
            this.Close();
        }
        private void BtnBackToMenu_Click(object sender, EventArgs e)
        {
            ShowPanel(FormModes.MainMenu);
        }        
        private void BtnDoSearch_Click(object sender, EventArgs e) // выполнение поиска
        {
            ComboBox cmbColumn = FindControlInPanel(panelSearch, "cmbSearchColumn") as ComboBox;
            TextBox txtValue = FindControlInPanel(panelSearch, "txtSearchValue") as TextBox;

            if (cmbColumn == null || txtValue == null) 
                return;
            if (string.IsNullOrWhiteSpace(txtValue.Text))
            {
                ErrorHandler.ShowWarning("Введите поисковый запрос!");
                return;
            }
            try
            {
                _currentData = _tableOps.Search(_currentTableName, cmbColumn.SelectedItem.ToString(), txtValue.Text.Trim());
                dataGridViewMain.DataSource = _currentData;
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "Search");
            }
        }        
        private void BtnApplyFilter_Click(object sender, EventArgs e) // применение фильтра
        {
            ComboBox cmbColumn = FindControlInPanel(panelFilter, "cmbFilterColumn") as ComboBox;
            ComboBox cmbValue = FindControlInPanel(panelFilter, "cmbFilterValue") as ComboBox;
            if (cmbColumn == null || cmbValue == null) 
                return;

            if (cmbValue.SelectedIndex == 0)
            {
                LoadTableData();
                return;
            }
            string filterText = cmbValue.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(filterText))
            {
                ErrorHandler.ShowWarning("Выберите значение для фильтрации!");
                return;
            }
            try
            {
                _currentData = _tableOps.Filter(_currentTableName, cmbColumn.SelectedItem.ToString(), filterText);
                dataGridViewMain.DataSource = _currentData;
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "Filter");
            }
        }        
        private void BtnSortAsc_Click(object sender, EventArgs e) // сортировка по возрастанию
        {
            PerformSort(true);
        }        
        private void BtnSortDesc_Click(object sender, EventArgs e) // сортировка по убыванию
        {
            PerformSort(false);
        }       
        private void PerformSort(bool ascending) // выполнение сортировки
        {
            ComboBox cmbColumn = FindControlInPanel(panelSort, "cmbSortColumn") as ComboBox;
            if (cmbColumn?.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите поле для сортировки!");
                return;
            }
            try
            {
                _currentData = _tableOps.Sort(_currentTableName, cmbColumn.SelectedItem.ToString(), ascending);
                dataGridViewMain.DataSource = _currentData;
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "Sort");
            }
        }        
        private void BtnShowAll_Click(object sender, EventArgs e) // показ всех записей
        {
            LoadTableData();
        }
        private void CreateDynamicForm(Panel panel, DataGridViewRow row, bool isAdd)
        {
            panel.Controls.Clear();
            List<string> columns = _dbHelper.GetColumnNames(_currentTableName);
            var autoNumberColumns = _tableOps.GetAutoNumberColumns(_currentTableName);
            var fkMappings = _tableOps.GetForeignKeyMappings(_currentTableName);

            int y = 5;
            panel.Controls.Add(PanelBuilder.CreateLabel(isAdd ? UIStyles.AddRecordTitle : UIStyles.EditRecordTitle, new Point(0, y), true));
            y += 25;

            var textBoxes = new Dictionary<string, TextBox>();
            var comboBoxes = new Dictionary<string, ComboBox>();

            foreach (string col in columns)
            {
                if (ShouldSkipColumn(col)) continue;

                AddLabelToPanel(panel, col + ":", ref y, false);

                var context = new FieldContext
                {
                    Panel = panel,
                    ColumnName = col,
                    Row = row,
                    IsAddMode = isAdd,
                    AutoNumberColumns = autoNumberColumns,
                    ForeignKeyMappings = fkMappings,
                    TextBoxes = textBoxes,
                    ComboBoxes = comboBoxes,
                    CurrentY = y
                };
                CreateFieldControl(context);
                y = context.CurrentY + 35;
            }
            AddSubmitButton(panel, textBoxes, comboBoxes, isAdd, row, ref y);
            AddButtonToPanel(panel, "Вернуться назад", ref y, UIStyles.NeutralButton, BtnBackToMenu_Click);
        }
        private bool ShouldSkipColumn(string col) // проверка, нужно ли пропускать столбец
        {
            return _currentTableName == TableConstants.TableUsers && col == TableConstants.FieldPasswordHash;
        }        
        private void CreateFieldControl(FieldContext context) // создание элемента управления для поля
        {
            if (_currentTableName == TableConstants.TableUsers && context.ColumnName == TableConstants.FieldRole)
            {
                CreateReadOnlyRoleField(context);
                return;
            }

            if (context.AutoNumberColumns.Contains(context.ColumnName))
            {
                CreateAutoNumberField(context);
            }
            else if (context.ForeignKeyMappings.ContainsKey(context.ColumnName))
            {
                CreateForeignKeyField(context);
            }
            else
            {
                CreateRegularField(context);
            }
        }
        private void CreateReadOnlyRoleField(FieldContext context)
        {
            TextBox txt = PanelBuilder.CreateTextBox(new Point(0, context.CurrentY));
            txt.Name = "txt_" + context.ColumnName;
            txt.ReadOnly = true;
            txt.BackColor = Color.FromArgb(240, 240, 240);

            if (!context.IsAddMode && context.Row != null && context.Row.Cells[context.ColumnName].Value != null)
            {
                txt.Text = context.Row.Cells[context.ColumnName].Value.ToString();
            }
            else
            {
                txt.Text = TableConstants.RoleGuest;
            }

            context.TextBoxes[context.ColumnName] = txt;
            context.Panel.Controls.Add(txt);
        }
        private void CreateAutoNumberField(FieldContext context)
        {
            TextBox txt = PanelBuilder.CreateTextBox(new Point(0, context.CurrentY));
            txt.Name = "txt_" + context.ColumnName;
            txt.ReadOnly = true;
            txt.BackColor = Color.FromArgb(240, 240, 240);

            if (!context.IsAddMode && context.Row != null)
            {
                object value = context.Row.Cells[context.ColumnName].Value;
                txt.Text = DataFormatter.FormatValueForDisplay(value);
            }
            else
            {
                txt.Text = UIStyles.AutoValueText;
            }

            context.TextBoxes[context.ColumnName] = txt;
            context.Panel.Controls.Add(txt);
        }
        private void CreateForeignKeyField(FieldContext context)
        {
            ComboBox cmb = PanelBuilder.CreateComboBox(new Point(0, context.CurrentY));
            cmb.Name = "cmb_" + context.ColumnName;

            try
            {
                string relatedTable = context.ForeignKeyMappings[context.ColumnName];
                FillForeignKeyComboBox(cmb, relatedTable, context.ColumnName, context.Row, context.IsAddMode);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка создания поля FK {context.ColumnName}: {ex.Message}", ex, "CreateForeignKeyField");
            }

            context.ComboBoxes[context.ColumnName] = cmb;
            context.Panel.Controls.Add(cmb);
        }
        private void FillForeignKeyComboBox(ComboBox cmb, string relatedTable, string col, DataGridViewRow row, bool isAdd) // заполнение ComboBox для внешнего ключа
        {
            var relatedData = _dbHelper.GetAllData(relatedTable);

            if (relatedData.Columns.Count >= 2)
            {
                string codeColumn = relatedData.Columns[0].ColumnName;
                string nameColumn = relatedData.Columns[1].ColumnName;

                cmb.Items.Add(UIStyles.NotSelectedText);
                foreach (DataRow relatedRow in relatedData.Rows)
                {
                    string code = relatedRow[codeColumn].ToString();
                    string name = relatedRow[nameColumn].ToString();
                    cmb.Items.Add($"{code} - {name}");
                }
                SelectCurrentValue(cmb, col, row, isAdd);
            }
        }        
        private void SelectCurrentValue(ComboBox cmb, string col, DataGridViewRow row, bool isAdd) // выбор текущего значения в ComboBox
        {
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
        private void CreateRegularField(FieldContext context)
        {
            TextBox txt = PanelBuilder.CreateTextBox(new Point(0, context.CurrentY));
            txt.Name = "txt_" + context.ColumnName;

            if (!context.IsAddMode && context.Row != null)
            {
                object value = context.Row.Cells[context.ColumnName].Value;
                txt.Text = DataFormatter.FormatValueForDisplay(value);
            }

            context.TextBoxes[context.ColumnName] = txt;
            context.Panel.Controls.Add(txt);
        }
        private void AddSubmitButton(Panel panel, Dictionary<string, TextBox> textBoxes, // добавление кнопки отправки формы
      Dictionary<string, ComboBox> comboBoxes, bool isAdd, DataGridViewRow row, ref int y)
        {
            string buttonText = isAdd ? "Добавить" : "Сохранить";

            var context = new SubmitContext
            {
                TextBoxes = textBoxes,
                ComboBoxes = comboBoxes,
                IsAddMode = isAdd,
                Row = row
            };

            AddButtonToPanel(panel, buttonText, ref y, UIStyles.SuccessButton,
                (s, ev) => BtnSubmitDynamic_Click(s, ev, context));
        }
        private void BtnSubmitDynamic_Click(object sender, EventArgs e, SubmitContext context) // обработчик динамической отправки 
        {
            try
            {
                var values = CollectFieldValues(context.TextBoxes, context.ComboBoxes);

                if (context.IsAddMode)
                {
                    AddNewRecord(values);
                }
                else
                {
                    UpdateExistingRecord(values, context.Row);
                }

                LoadTableData();
                ShowPanel(FormModes.MainMenu);
                ForceCleanupComResources();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка в SubmitDynamic: {ex.Message}", ex, "BtnSubmitDynamic_Click");
                ErrorHandler.Handle(ex, "SubmitDynamic");
            }
        }
        private Dictionary<string, object> CollectFieldValues(Dictionary<string, TextBox> textBoxes, // сбор значений полей
            Dictionary<string, ComboBox> comboBoxes) 
        {
            var values = new Dictionary<string, object>();

            foreach (var kvp in textBoxes)
            {
                string val = kvp.Value.Text.Trim();
                values[kvp.Key] = string.IsNullOrEmpty(val) ? DBNull.Value : (object)val;
            }

            foreach (var kvp in comboBoxes)
            {
                values[kvp.Key] = ExtractComboBoxValue(kvp.Value);
            }

            return values;
        }        
        private object ExtractComboBoxValue(ComboBox cmb) // извлечение значения из ComboBox
        {
            if (cmb.SelectedIndex <= 0)
                return DBNull.Value;

            string selectedText = cmb.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedText))
                return DBNull.Value;

            if (selectedText.Contains(" - "))
            {
                return selectedText.Split(new[] { " - " }, StringSplitOptions.None)[0];
            }

            return (object)selectedText;
        }        
        private void AddNewRecord(Dictionary<string, object> values) // добавление новой записи
        {
            if (!CheckUniqueConstraints(values)) 
                return;

            var autoNumberColumns = _tableOps.GetAutoNumberColumns(_currentTableName);
            _tableOps.AddRecord(_currentTableName, values, autoNumberColumns);
            ErrorHandler.ShowInfo("Запись успешно добавлена!");

            LoadTableData();
            if (dataGridViewMain.Rows.Count > 0)
            {
                dataGridViewMain.CurrentCell = dataGridViewMain.Rows[0].Cells[0];
            }
            ShowPanel(FormModes.MainMenu);
        }        
        private bool CheckUniqueConstraints(Dictionary<string, object> values) // проверка уникальных ограничений
        {
            var uniqueColumns = _dbHelper.GetUniqueColumns(_currentTableName);

            foreach (var uniqueCol in uniqueColumns)
            {
                if (values.ContainsKey(uniqueCol) && values[uniqueCol] != DBNull.Value)
                {
                    if (IsValueExists(uniqueCol, values[uniqueCol]))
                    {
                        ErrorHandler.ShowWarning(
                            $"Значение поля \"{uniqueCol}\" уже существует!\n\n" +
                            $"Измените значение и попробуйте снова.");
                        return false;
                    }
                }
            }
            return true;
        }        
        private bool IsValueExists(string columnName, object value)// проверка существования значения
        {
            string checkQuery = $"SELECT COUNT(*) FROM [{_currentTableName}] WHERE [{columnName}] = ?";
            int count = Convert.ToInt32(_dbHelper.ExecuteScalar(checkQuery,
                new System.Data.OleDb.OleDbParameter[] { new System.Data.OleDb.OleDbParameter("?", value) }));
            return count > 0;
        }        
        private void UpdateExistingRecord(Dictionary<string, object> values, DataGridViewRow row) // обновление существующей записи
        {
            object keyValue = row.Cells[_primaryKeyColumn].Value;
            _tableOps.UpdateRecord(_currentTableName, values, _primaryKeyColumn, keyValue, _primaryKeyColumn);
            ErrorHandler.ShowInfo("Запись успешно обновлена!");
        }
        private void BtnDoDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewMain.CurrentRow == null)
            {
                ErrorHandler.ShowWarning("Выберите запись для удаления!");
                return;
            }
            try
            {
                if (_currentTableName == TableConstants.TableUsers)
                {
                    DeleteUser();
                }
                else
                {
                    DeleteRecordWithCascade();
                }
                ForceCleanupComResources();
            }
            catch (Exception ex)
            {
                ForceCleanupComResources(); 
                ErrorHandler.Handle(ex, "Delete");
            }
        }
        private void DeleteUser() // удаление пользователя
        {
            string loginToDelete = dataGridViewMain.CurrentRow.Cells[TableConstants.FieldLogin].Value?.ToString();
            string roleToDelete = dataGridViewMain.CurrentRow.Cells[TableConstants.FieldRole].Value?.ToString();

            if (!CanDeleteUser(loginToDelete, roleToDelete)) return;

            if (ErrorHandler.AskQuestion($"Вы уверены, что хотите удалить пользователя \"{loginToDelete}\"?\n\nВсе данные, связанные с этим пользователем, будут удалены!"))
            {
                PerformUserDeletion(loginToDelete);
            }
        }        
        private bool CanDeleteUser(string loginToDelete, string roleToDelete) // проверка возможности удаления пользователя
        {
            if (loginToDelete == _userService.GetDatabaseFileName())
            {
                ErrorHandler.ShowWarning("НЕЛЬЗЯ УДАЛИТЬ:\nНельзя удалить самого себя!");
                return false;
            }
            if (roleToDelete == TableConstants.RoleAdmin && !_userService.CanDeleteUser(loginToDelete, _currentUserRole))
            {
                ErrorHandler.ShowWarning(
                    "НЕЛЬЗЯ УДАЛИТЬ:\n\n" +
                    "- Нельзя удалить последнего администратора\n" +
                    "- В системе должен остаться хотя бы один админ!");
                return false;
            }
            return true;
        }        
        private void PerformUserDeletion(string loginToDelete) // выполнение удаления пользователя
        {
            try
            {
                object keyValue = dataGridViewMain.CurrentRow.Cells[_primaryKeyColumn].Value;
                _tableOps.DeleteRecord(_currentTableName, _primaryKeyColumn, keyValue);
                ErrorHandler.ShowInfo($"Пользователь \"{loginToDelete}\" удален!");
                LoadTableData();
                ShowPanel(FormModes.MainMenu);
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "DeleteUser");
            }
        }        
        private void DeleteRecordWithCascade() // удаление записи с каскадом
        {
            bool needsCascade = NeedsCascadeDelete(_currentTableName);
            string message = "Вы уверены, что хотите удалить выбранную запись?";

            if (needsCascade)
            {
                message += $"\n\n ВНИМАНИЕ:\n{GetCascadeWarning(_currentTableName)}\n\nВсе связанные записи будут УДАЛЕНЫ!";
            }

            if (ErrorHandler.AskQuestion(message))
            {
                PerformRecordDeletion(needsCascade);
            }
        }       
        private void PerformRecordDeletion(bool needsCascade)  // уыполнение удаления записи
        {
            try
            {
                object keyValue = dataGridViewMain.CurrentRow.Cells[_primaryKeyColumn].Value;

                if (needsCascade)
                {
                    _tableOps.DeleteWithCascade(_currentTableName, _primaryKeyColumn, keyValue);
                    ErrorHandler.ShowInfo("Запись и все связанные записи удалены!");
                }
                else
                {
                    _tableOps.DeleteRecord(_currentTableName, _primaryKeyColumn, keyValue);
                    ErrorHandler.ShowInfo("Запись удалена!");
                }
                LoadTableData();
                ShowPanel(FormModes.MainMenu);
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "Delete");
            }
        }        
        private bool NeedsCascadeDelete(string tableName) // проверка необходимости каскадного удаления
        {
            return tableName == TableConstants.TableFaculties ||
                   tableName == TableConstants.TableSpecialties ||
                   tableName == TableConstants.TableSpecializations ||
                   tableName == TableConstants.TableSchools;
        }       
        private string GetCascadeWarning(string tableName) // получение предупреждения о каскадном удалении
        {
            return tableName switch
            {
                TableConstants.TableFaculties => "Будут удалены:\n- Все специальности факультета\n- Все специализации\n- Все абитуриенты",
                TableConstants.TableSpecialties => "Будут удалены:\n- Все специализации специальности\n- Все абитуриенты",
                TableConstants.TableSpecializations => "Будут удалены:\n- Все абитуриенты этой специализации",
                TableConstants.TableSchools => "Будут удалены:\n- Все абитуриенты этой школы",
                _ => null
            };
        }        
        private void BtnCreateReport_Click(object sender, EventArgs e) // создание отчёта
        {
            ComboBox cmbFormat = FindControlInPanel(panelReport, "cmbReportFormat") as ComboBox;
            string format = cmbFormat?.SelectedItem?.ToString() ?? "Word (.doc)";
            try
            {
                string filePath = ShowSaveFileDialog(format);
                if (!string.IsNullOrEmpty(filePath))
                {
                    GenerateReport(format, filePath);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "CreateReport");
            }
        }       
        private string ShowSaveFileDialog(string format)  // показ диалога сохранения файла
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = format.Contains("Word") ? "Word Document|*.doc" : "Text File|*.txt";
                saveDialog.DefaultExt = format.Contains("Word") ? ".doc" : ".txt";
                saveDialog.FileName = $"Отчёт_{_currentTableName}_{DateTime.Now:yyyyMMdd}";

                return saveDialog.ShowDialog() == DialogResult.OK ? saveDialog.FileName : null;
            }
        }        
        private void GenerateReport(string format, string filePath) // генерация отчёта
        {
            if (format.Contains("Word"))
            {
                ReportGenerator.CreateWordReport(_currentData, _currentTableName, _currentUserRole, filePath);
            }
            else
            {
                ReportGenerator.CreateTextReport(_currentData, _currentTableName, _currentUserRole, filePath);
            }
            ErrorHandler.ShowInfo("Отчёт успешно создан!");
        }        
        private bool CheckTableSelected() // проверка выбора таблицы
        {
            if (string.IsNullOrEmpty(_currentTableName))
            {
                ErrorHandler.ShowWarning("Сначала выберите таблицу!");
                return false;
            }
            return true;
        }        
        private void ForceCleanupComResources() // принудительная очистка COM-ресурсов (должна по идее предотвращать внезапный вылет)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch { }
        }
    }
}