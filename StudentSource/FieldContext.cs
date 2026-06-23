using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace PractiStudent
{
    public class FieldContext // контекст для создания полей формы, инкапсулирует множество параметров методов в один объект
    {
        public Panel Panel { get; set; }
        public string ColumnName { get; set; }
        public DataGridViewRow Row { get; set; }
        public bool IsAddMode { get; set; }
        public List<string> AutoNumberColumns { get; set; }
        public Dictionary<string, string> ForeignKeyMappings { get; set; }
        public Dictionary<string, TextBox> TextBoxes { get; set; }
        public Dictionary<string, ComboBox> ComboBoxes { get; set; }
        public int CurrentY { get; set; }
    }
}