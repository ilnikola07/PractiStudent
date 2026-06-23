using System.Collections.Generic;
using System.Data;

namespace PractiStudent
{
    public class SubmitContext  // инкапсуляция параметров обработчика отправки формы
    {
        public Dictionary<string, TextBox> TextBoxes { get; set; }
        public Dictionary<string, ComboBox> ComboBoxes { get; set; }
        public bool IsAddMode { get; set; }
        public DataGridViewRow Row { get; set; }
    }
}