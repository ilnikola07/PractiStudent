using Exceptions;
using Logic;
using PractiStudent.Data;
using StudentSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PractiStudent
{
    public partial class FormMain : Form
    {
        // Поля класса
        private readonly string _currentUserRole;
        private readonly string _databaseFileName;
        private readonly UserService _userService;
        private readonly DatabaseHelper _dbHelper;
        private readonly TableOperations _tableOps;

        private string _currentTableName = "";
        private DataTable _currentData;
        private string _primaryKeyColumn = "";

        // UI элементы
        private ComboBox cmbTables;
        private Label lblTableSelect;
        private Label lblConnectionInfo;

        // Панели действий
        private Panel panelMainMenu;
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
                const int CS_NOCLOSE = 0x200;
                cp.ClassStyle |= CS_NOCLOSE;
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
            _dbHelper.SetDatabaseConnection(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName));
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

        private void ConfigureForm()
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

        private void InitializeDataGridView()
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

        private void InitializeConnectionIndicator()
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

        private void InitializeLeftPanel()
        {
            lblTableSelect = PanelBuilder.CreateLabel("Выберите таблицу:", UIStyles.TableSelectLabelPosition, true);

            cmbTables = PanelBuilder.CreateComboBox(UIStyles.TableSelectComboPosition);
            cmbTables.Size = UIStyles.TableSelectComboSize;
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
                    if (table.Equals(TableConstants.TableUsers, StringComparison.OrdinalIgnoreCase))
                    {
                        if (_currentUserRole == TableConstants.RoleAdmin)
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
                    ErrorHandler.ShowInfo("Нет доступных таблиц для просмотра.");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "LoadTables");
            }
        }

        private void CmbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTables.SelectedItem != null)
            {
                _currentTableName = cmbTables.SelectedItem.ToString();
                ClearActionPanels();
                LoadTableData();
                ShowPanel(FormModes.MainMenu);
            }
        }

        private void LoadTableData()
        {
            if (string.IsNullOrEmpty(_currentTableName)) return;
            try
            {
                _currentData = _dbHelper.GetAllData(_currentTableName);
                DataFormatter.FormatDatesInTable(_currentData);
                dataGridViewMain.DataSource = _currentData;
                if (_currentTableName == TableConstants.TableUsers)
                {
                    dataGridViewMain.Columns["Хэш_пароля"].Visible = false;
                }

                if (_currentData.Columns.Count > 0)
                {
                    _primaryKeyColumn = _currentData.Columns[0].ColumnName;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "LoadTableData");
            }
        }

        private void CreateAllActionPanels()
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

        private Panel CreatePanel(string mode)
        {
            Panel panel = PanelBuilder.CreateActionPanel(UIStyles.ActionPanelTop, UIStyles.ActionPanelHeight);
            splitContainer1.Panel1.Controls.Add(panel);
            return panel;
        }

        private void ShowPanel(string mode)
        {
            foreach (Control ctrl in splitContainer1.Panel1.Controls)
            {
                if (ctrl is Panel p)
                {
                    p.Visible = (p == GetPanelByMode(mode));
                    if (!p.Visible)
                    {
                        p.Controls.Clear();
                    }
                }
            }

            RebuildPanel(mode);
            GetPanelByMode(mode).Visible = true;
            PopulateColumnComboBoxes();
        }

        private Panel GetPanelByMode(string mode)
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

        private void RebuildPanel(string mode)
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

        private void ClearActionPanels()
        {
            panelAdd?.Controls.Clear();
            panelEdit?.Controls.Clear();
            panelDelete?.Controls.Clear();
        }

        private void CreateMainMenuButtons()
        {
            panelMainMenu.Controls.Clear();
            int y = UIStyles.ActionButtonPosition.Y;

            panelMainMenu.Controls.Add(PanelBuilder.CreateButton("Поиск", new Point(0, y), UIStyles.PrimaryButton, BtnSearch_Click));
            y += UIStyles.ButtonVerticalSpacing;

            panelMainMenu.Controls.Add(PanelBuilder.CreateButton("Фильтрация", new Point(0, y), UIStyles.PrimaryButton, BtnFilter_Click));
            y += UIStyles.ButtonVerticalSpacing;

            panelMainMenu.Controls.Add(PanelBuilder.CreateButton("Сортировка", new Point(0, y), UIStyles.PrimaryButton, BtnSort_Click));
            y += UIStyles.ButtonVerticalSpacing;

            if (_currentUserRole == TableConstants.RoleAdmin)
            {
                panelMainMenu.Controls.Add(PanelBuilder.CreateButton("Добавить запись", new Point(0, y), UIStyles.PrimaryButton, BtnAdd_Click));
                y += UIStyles.ButtonVerticalSpacing;

                panelMainMenu.Controls.Add(PanelBuilder.CreateButton("Редактировать запись", new Point(0, y), UIStyles.PrimaryButton, BtnEdit_Click));
                y += UIStyles.ButtonVerticalSpacing;

                panelMainMenu.Controls.Add(PanelBuilder.CreateButton("Удалить запись", new Point(0, y), UIStyles.PrimaryButton, BtnDelete_Click));
                y += UIStyles.ButtonVerticalSpacing;
            }

            panelMainMenu.Controls.Add(PanelBuilder.CreateButton("Создать отчёт (Word)", new Point(0, y), UIStyles.PrimaryButton, BtnReport_Click));
            y += 60;

            panelMainMenu.Controls.Add(PanelBuilder.CreateButton("Выйти из приложения", new Point(0, y), UIStyles.DangerButton, BtnExit_Click));
            y += UIStyles.ButtonVerticalSpacing;

            panelMainMenu.Controls.Add(PanelBuilder.CreateButton("Выйти из аккаунта", new Point(0, y), UIStyles.NeutralButton, BtnLogout_Click));
        }

        private void CreateSearchPanel()
        {
            panelSearch.Controls.Clear();
            int y = 5;

            panelSearch.Controls.Add(PanelBuilder.CreateLabel("Поле для поиска:", new Point(0, y), true));
            ComboBox cmbColumn = PanelBuilder.CreateComboBox(new Point(0, y + 23));
            cmbColumn.Name = "cmbSearchColumn";
            panelSearch.Controls.Add(cmbColumn);

            panelSearch.Controls.Add(PanelBuilder.CreateLabel("Поисковый запрос:", new Point(0, y + 60), true));
            TextBox txtSearch = PanelBuilder.CreateTextBox(new Point(0, y + 83));
            txtSearch.Name = "txtSearchValue";
            panelSearch.Controls.Add(txtSearch);

            panelSearch.Controls.Add(PanelBuilder.CreateButton("Найти", new Point(0, y + 120), UIStyles.SuccessButton, BtnDoSearch_Click));
            panelSearch.Controls.Add(PanelBuilder.CreateButton("Показать все", new Point(0, y + 170), UIStyles.PrimaryButton, BtnShowAll_Click));
            panelSearch.Controls.Add(PanelBuilder.CreateButton("Вернуться назад", new Point(0, y + 275), UIStyles.NeutralButton, BtnBackToMenu_Click));
        }

        private void CreateFilterPanel()
        {
            panelFilter.Controls.Clear();
            int y = 5;

            panelFilter.Controls.Add(PanelBuilder.CreateLabel("Поле для фильтрации:", new Point(0, y), true));
            ComboBox cmbColumn = PanelBuilder.CreateComboBox(new Point(0, y + 23));
            cmbColumn.Name = "cmbFilterColumn";
            cmbColumn.SelectedIndexChanged += CmbFilterColumn_SelectedIndexChanged;  
            panelFilter.Controls.Add(cmbColumn);

            panelFilter.Controls.Add(PanelBuilder.CreateLabel("Значение:", new Point(0, y + 60), true));

            ComboBox cmbValue = PanelBuilder.CreateComboBox(new Point(0, y + 83));
            cmbValue.Name = "cmbFilterValue";
            cmbValue.Items.Add("(все значения)");  // Первый элемент - показать все
            panelFilter.Controls.Add(cmbValue);

            panelFilter.Controls.Add(PanelBuilder.CreateButton("Применить фильтр", new Point(0, y + 120), UIStyles.SuccessButton, BtnApplyFilter_Click));
            panelFilter.Controls.Add(PanelBuilder.CreateButton("Сбросить фильтр", new Point(0, y + 170), UIStyles.PrimaryButton, BtnShowAll_Click));
            panelFilter.Controls.Add(PanelBuilder.CreateButton("Вернуться назад", new Point(0, y + 275), UIStyles.NeutralButton, BtnBackToMenu_Click));
        }

        private void CreateSortPanel()
        {
            panelSort.Controls.Clear();
            int y = 5;

            panelSort.Controls.Add(PanelBuilder.CreateLabel("Поле для сортировки:", new Point(0, y), true));
            ComboBox cmbColumn = PanelBuilder.CreateComboBox(new Point(0, y + 23));
            cmbColumn.Name = "cmbSortColumn";
            panelSort.Controls.Add(cmbColumn);

            panelSort.Controls.Add(PanelBuilder.CreateButton("По возрастанию", new Point(0, y + 65), UIStyles.SuccessButton, BtnSortAsc_Click));
            panelSort.Controls.Add(PanelBuilder.CreateButton("По убыванию", new Point(0, y + 115), UIStyles.SuccessButton, BtnSortDesc_Click));
            panelSort.Controls.Add(PanelBuilder.CreateButton("Сбросить сортировку", new Point(0, y + 165), UIStyles.PrimaryButton, BtnShowAll_Click));
            panelSort.Controls.Add(PanelBuilder.CreateButton("Вернуться назад", new Point(0, y + 270), UIStyles.NeutralButton, BtnBackToMenu_Click));
        }

        private void CreateDeletePanel()
        {
            panelDelete.Controls.Clear();
            int y = 5;

            panelDelete.Controls.Add(PanelBuilder.CreateLabel("Выберите запись в таблице справа", new Point(0, y), true));
            panelDelete.Controls.Add(PanelBuilder.CreateLabel("и нажмите кнопку ниже для удаления.", new Point(0, y + 23), false));
            panelDelete.Controls.Add(PanelBuilder.CreateButton("Удалить выбранную запись", new Point(0, y + 65), UIStyles.DangerButton, BtnDoDelete_Click));
            panelDelete.Controls.Add(PanelBuilder.CreateButton("Вернуться назад", new Point(0, y + 270), UIStyles.NeutralButton, BtnBackToMenu_Click));
        }

        private void CreateReportPanel()
        {
            panelReport.Controls.Clear();
            int y = 5;

            panelReport.Controls.Add(PanelBuilder.CreateLabel("Формат отчёта:", new Point(0, y), true));
            ComboBox cmbFormat = PanelBuilder.CreateComboBox(new Point(0, y + 23));
            cmbFormat.Name = "cmbReportFormat";
            cmbFormat.Items.Add("Word (.doc)");
            cmbFormat.Items.Add("Текстовый файл (.txt)");
            cmbFormat.SelectedIndex = 0;
            panelReport.Controls.Add(cmbFormat);

            panelReport.Controls.Add(PanelBuilder.CreateLabel("Отчёт будет содержать все данные", new Point(0, y + 65)));
            panelReport.Controls.Add(PanelBuilder.CreateLabel("текущей выбранной таблицы.", new Point(0, y + 88)));
            panelReport.Controls.Add(PanelBuilder.CreateButton("Сформировать отчёт", new Point(0, y + 125), UIStyles.SuccessButton, BtnCreateReport_Click));
            panelReport.Controls.Add(PanelBuilder.CreateButton("Вернуться назад", new Point(0, y + 270), UIStyles.NeutralButton, BtnBackToMenu_Click));
        }

        private void PopulateColumnComboBoxes()
        {
            if (string.IsNullOrEmpty(_currentTableName)) return;

            try
            {
                List<string> columns = _dbHelper.GetColumnNames(_currentTableName);

                foreach (Control ctrl in splitContainer1.Panel1.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        foreach (Control child in panel.Controls)
                        {
                            if (child is ComboBox cmb &&
                                (cmb.Name == "cmbSearchColumn" || cmb.Name == "cmbFilterColumn" || cmb.Name == "cmbSortColumn"))
                            {
                                cmb.Items.Clear();
                                cmb.Items.AddRange(columns.ToArray());
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

            ComboBox cmbValue = FindControlInPanel(panelFilter, "cmbFilterValue") as ComboBox;
            if (cmbValue == null) return;

            try
            {
                string columnName = cmbColumn.SelectedItem.ToString();
                List<string> values = _dbHelper.GetDistinctValues(_currentTableName, columnName);

                cmbValue.Items.Clear();
                cmbValue.Items.Add("(все значения)");

                foreach (string val in values)
                {
                    if (!string.IsNullOrEmpty(val))
                    {
                        string displayValue = DataFormatter.FormatValueForDisplay(val);
                        cmbValue.Items.Add(displayValue);
                    }
                }

                cmbValue.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка загрузки значений для фильтра: {ex.Message}", ex, "CmbFilterColumn_SelectedIndexChanged");
            }
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
            if (CheckTableSelected()) ShowPanel(FormModes.Search);
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            if (CheckTableSelected()) ShowPanel(FormModes.Filter);
        }

        private void BtnSort_Click(object sender, EventArgs e)
        {
            if (CheckTableSelected()) ShowPanel(FormModes.Sort);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (CheckTableSelected())
            {
                if (_currentTableName == TableConstants.TableUsers)
                {
                    ErrorHandler.ShowWarning(
                        "Добавление новых пользователей запрещено!\n\n" +
                        "Пользователи создаются автоматически при регистрации через форму входа.");
                    return;
                }

                ShowPanel(FormModes.Add);
            }
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
            if (CheckTableSelected()) ShowPanel(FormModes.Delete);
        }

        private void BtnReport_Click(object sender, EventArgs e)
        {
            if (CheckTableSelected()) ShowPanel(FormModes.Report);
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
            ShowPanel(FormModes.MainMenu);
        }

        private void BtnDoSearch_Click(object sender, EventArgs e)
        {
            ComboBox cmbColumn = FindControlInPanel(panelSearch, "cmbSearchColumn") as ComboBox;
            TextBox txtValue = FindControlInPanel(panelSearch, "txtSearchValue") as TextBox;

            if (cmbColumn == null || txtValue == null) return;
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

        private void BtnApplyFilter_Click(object sender, EventArgs e)
        {
            ComboBox cmbColumn = FindControlInPanel(panelFilter, "cmbFilterColumn") as ComboBox;
            ComboBox cmbValue = FindControlInPanel(panelFilter, "cmbFilterValue") as ComboBox;  // 

            if (cmbColumn == null || cmbValue == null) return;

            // Если выбрано "(все значения)" - показываем все
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
                // ТОЧНАЯ фильтрация
                _currentData = _tableOps.Filter(_currentTableName, cmbColumn.SelectedItem.ToString(), filterText);
                dataGridViewMain.DataSource = _currentData;
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "Filter");
            }
        }

        private void BtnSortAsc_Click(object sender, EventArgs e)
        {
            ComboBox cmbColumn = FindControlInPanel(panelSort, "cmbSortColumn") as ComboBox;
            if (cmbColumn == null || cmbColumn.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите поле для сортировки!");
                return;
            }

            try
            {
                _currentData = _tableOps.Sort(_currentTableName, cmbColumn.SelectedItem.ToString(), true);
                dataGridViewMain.DataSource = _currentData;
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "Sort");
            }
        }

        private void BtnSortDesc_Click(object sender, EventArgs e)
        {
            ComboBox cmbColumn = FindControlInPanel(panelSort, "cmbSortColumn") as ComboBox;
            if (cmbColumn == null || cmbColumn.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите поле для сортировки!");
                return;
            }

            try
            {
                _currentData = _tableOps.Sort(_currentTableName, cmbColumn.SelectedItem.ToString(), false);
                dataGridViewMain.DataSource = _currentData;
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "Sort");
            }
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
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
            string title = isAdd ? UIStyles.AddRecordTitle : UIStyles.EditRecordTitle;
            panel.Controls.Add(PanelBuilder.CreateLabel(title, new Point(0, y), true));
            y += 25;

            Dictionary<string, TextBox> textBoxes = new Dictionary<string, TextBox>();
            Dictionary<string, ComboBox> comboBoxes = new Dictionary<string, ComboBox>();

            foreach (string col in columns)
            {
                if (_currentTableName == TableConstants.TableUsers && col == "Хэш_пароля")
                {
                    continue;
                }
                bool isAutoNumber = autoNumberColumns.Contains(col);
                bool isForeignKey = fkMappings.ContainsKey(col);

                panel.Controls.Add(PanelBuilder.CreateLabel(col + ":", new Point(0, y)));
                y += 20;

                if (isAutoNumber)
                {
                    TextBox txt = PanelBuilder.CreateTextBox(new Point(0, y));
                    txt.Name = "txt_" + col;
                    txt.ReadOnly = true;
                    txt.BackColor = Color.FromArgb(240, 240, 240);
                    txt.Text = (!isAdd && row != null) ? row.Cells[col].Value?.ToString() ?? "" : UIStyles.AutoValueText;
                    textBoxes[col] = txt;
                    panel.Controls.Add(txt);
                }
                else if (isForeignKey)
                {
                    ComboBox cmb = PanelBuilder.CreateComboBox(new Point(0, y));
                    cmb.Name = "cmb_" + col;

                    try
                    {
                        string relatedTable = fkMappings[col];
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
                    }
                    catch { }

                    comboBoxes[col] = cmb;
                    panel.Controls.Add(cmb);
                }
                else
                {
                    TextBox txt = PanelBuilder.CreateTextBox(new Point(0, y));
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

            string buttonText = isAdd ? "Добавить" : "Сохранить";
            panel.Controls.Add(PanelBuilder.CreateButton(buttonText, new Point(0, y + 10), UIStyles.SuccessButton,
                (s, ev) => BtnSubmitDynamic_Click(s, ev, textBoxes, comboBoxes, isAdd, row)));
            panel.Controls.Add(PanelBuilder.CreateButton("Вернуться назад", new Point(0, y + 60), UIStyles.NeutralButton, BtnBackToMenu_Click));
        }

        private void BtnSubmitDynamic_Click(object sender, EventArgs e,
     Dictionary<string, TextBox> textBoxes, Dictionary<string, ComboBox> comboBoxes,
     bool isAdd, DataGridViewRow row)
        {
            try
            {
                var values = new Dictionary<string, object>();
                var autoNumberColumns = _tableOps.GetAutoNumberColumns(_currentTableName);

                foreach (var kvp in textBoxes)
                {
                    string val = kvp.Value.Text.Trim();
                    values[kvp.Key] = string.IsNullOrEmpty(val) ? DBNull.Value : (object)val;
                }

                foreach (var kvp in comboBoxes)
                {
                    ComboBox cmb = kvp.Value;
                    if (cmb.SelectedIndex <= 0)
                    {
                        values[kvp.Key] = DBNull.Value;
                    }
                    else
                    {
                        string selectedText = cmb.SelectedItem?.ToString();
                        if (!string.IsNullOrEmpty(selectedText) && selectedText.Contains(" - "))
                        {
                            string code = selectedText.Split(new[] { " - " }, StringSplitOptions.None)[0];
                            values[kvp.Key] = code;
                        }
                        else
                        {
                            values[kvp.Key] = string.IsNullOrEmpty(selectedText) ? DBNull.Value : (object)selectedText;
                        }
                    }
                }

                if (isAdd)
                {
                    // ПРОВЕРКА: есть ли дубликаты в уникальных полях
                    var uniqueColumns = _dbHelper.GetUniqueColumns(_currentTableName);

                    foreach (var uniqueCol in uniqueColumns)
                    {
                        if (values.ContainsKey(uniqueCol) && values[uniqueCol] != DBNull.Value)
                        {
                            // Проверяем, есть ли уже такая запись
                            string checkQuery = $"SELECT COUNT(*) FROM [{_currentTableName}] WHERE [{uniqueCol}] = ?";
                            int count = Convert.ToInt32(_dbHelper.ExecuteScalar(checkQuery,
                                new OleDbParameter[] { new OleDbParameter("?", values[uniqueCol]) }));

                            if (count > 0)
                            {
                                ErrorHandler.ShowWarning(
                                    $"Значение поля \"{uniqueCol}\" уже существует!\n\n" +
                                    $"Измените значение и попробуйте снова.");
                                return;
                            }
                        }
                    }

                    _tableOps.AddRecord(_currentTableName, values, autoNumberColumns);
                    ErrorHandler.ShowInfo("Запись успешно добавлена!");
                }
                else
                {
                    object keyValue = row.Cells[_primaryKeyColumn].Value;
                    _tableOps.UpdateRecord(_currentTableName, values, _primaryKeyColumn, keyValue, _primaryKeyColumn);
                    ErrorHandler.ShowInfo("Запись успешно обновлена!");
                }

                LoadTableData();
                ShowPanel(FormModes.MainMenu);
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "SubmitDynamic");
            }
        }

        private void BtnDoDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewMain.CurrentRow == null)
            {
                ErrorHandler.ShowWarning("Выберите запись для удаления!");
                return;
            }

            // ?? Специальная проверка для таблицы Пользователи
            if (_currentTableName == TableConstants.TableUsers)
            {
                string loginToDelete = dataGridViewMain.CurrentRow.Cells["Логин"].Value?.ToString();
                string roleToDelete = dataGridViewMain.CurrentRow.Cells["Роль"].Value?.ToString();

                // Нельзя удалить самого себя
                if (loginToDelete == _userService.GetDatabaseFileName()) // или как вы храните текущего пользователя
                {
                    ErrorHandler.ShowWarning("НЕЛЬЗЯ УДАЛИТЬ:\nНельзя удалить самого себя!");
                    return;
                }

                // Нельзя удалить последнего администратора
                if (roleToDelete == TableConstants.RoleAdmin)
                {
                    if (!_userService.CanDeleteUser(loginToDelete, _currentUserRole))
                    {
                        ErrorHandler.ShowWarning(
                            "НЕЛЬЗЯ УДАЛИТЬ:\n\n" +
                            "- Нельзя удалить последнего администратора\n" +
                            "- В системе должен остаться хотя бы один админ!");
                        return;
                    }
                }

                if (ErrorHandler.AskQuestion(
                    $"Вы уверены, что хотите удалить пользователя \"{loginToDelete}\"?\n\n" +
                    $"Все данные, связанные с этим пользователем, будут удалены!"))
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
                return;
            }

            // Для остальных таблиц — обычное удаление с каскадом
            bool needsCascade = NeedsCascadeDelete(_currentTableName);

            string message = "Вы уверены, что хотите удалить выбранную запись?";
            if (needsCascade)
            {
                message += $"\n\n ВНИМАНИЕ:\n{GetCascadeWarning(_currentTableName)}\n\nВсе связанные записи будут УДАЛЕНЫ!";
            }

            if (ErrorHandler.AskQuestion(message))
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
        }

        // Вспомогательные методы
        private bool NeedsCascadeDelete(string tableName)
        {
            // Каскадное удаление для справочников
            return tableName == TableConstants.TableFaculties ||
                   tableName == TableConstants.TableSpecialties ||
                   tableName == TableConstants.TableSpecializations ||
                   tableName == TableConstants.TableSchools;
        }

        private string GetCascadeWarning(string tableName)
        {
            return tableName switch
            {
                TableConstants.TableFaculties =>
                    "Будут удалены:\n- Все специальности факультета\n- Все специализации\n- Все абитуриенты",

                TableConstants.TableSpecialties =>
                    "Будут удалены:\n- Все специализации специальности\n- Все абитуриенты",

                TableConstants.TableSpecializations =>
                    "Будут удалены:\n- Все абитуриенты этой специализации",

                TableConstants.TableSchools =>
                    "Будут удалены:\n- Все абитуриенты этой школы",

                _ => null
            };
        }
        private void BtnCreateReport_Click(object sender, EventArgs e)
        {
            ComboBox cmbFormat = FindControlInPanel(panelReport, "cmbReportFormat") as ComboBox;
            string format = cmbFormat?.SelectedItem?.ToString() ?? "Word (.doc)";

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = format.Contains("Word") ? "Word Document|*.doc" : "Text File|*.txt",
                    DefaultExt = format.Contains("Word") ? ".doc" : ".txt",
                    FileName = $"Отчёт_{_currentTableName}_{DateTime.Now:yyyyMMdd}"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (format.Contains("Word"))
                    {
                        ReportGenerator.CreateWordReport(_currentData, _currentTableName, _currentUserRole, saveDialog.FileName);
                    }
                    else
                    {
                        ReportGenerator.CreateTextReport(_currentData, _currentTableName, _currentUserRole, saveDialog.FileName);
                    }
                    ErrorHandler.ShowInfo("Отчёт успешно создан!");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "CreateReport");
            }
        }

        private bool CheckTableSelected()
        {
            if (string.IsNullOrEmpty(_currentTableName))
            {
                ErrorHandler.ShowWarning("Сначала выберите таблицу!");
                return false;
            }
            return true;
        }
    }
}