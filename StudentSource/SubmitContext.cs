using System.Collections.Generic;
using System.Data;

namespace PractiStudent
{
    public class SubmitContext
    {
        public Dictionary<string, TextBox> TextBoxes { get; set; }
        public Dictionary<string, ComboBox> ComboBoxes { get; set; }
        public bool IsAddMode { get; set; }
        public DataGridViewRow Row { get; set; }
    }
}