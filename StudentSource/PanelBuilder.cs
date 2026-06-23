using System;
using System.Drawing;
using System.Windows.Forms;

namespace PractiStudent
{
    public static class PanelBuilder // класс создания элементов на форме
    {
        public static Panel CreateActionPanel(int top, int height)
        {
            return new Panel
            {
                Location = new Point(15, top),
                Size = new Size(370, height),
                Visible = false
            };
        }
        public static Button CreateButton(string text, Point location, Color backColor, EventHandler clickEvent)
        {
            Button btn = new Button
            {
                Text = text,
                Font = UIStyles.ButtonFont,
                Location = location,
                Size = UIStyles.ButtonSize,
                BackColor = backColor,
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += clickEvent;
            return btn;
        }
        public static Label CreateLabel(string text, Point location, bool bold = false)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, bold ? FontStyle.Bold : FontStyle.Regular),
                Location = location,
                Size = new Size(280, 20)
            };
        }
        public static TextBox CreateTextBox(Point location)
        {
            return new TextBox
            {
                Font = UIStyles.InputFont,
                Location = location,
                Size = UIStyles.InputSize
            };
        }
        public static ComboBox CreateComboBox(Point location)
        {
            return new ComboBox
            {
                Font = UIStyles.InputFont,
                Location = location,
                Size = UIStyles.InputSize,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }
    }
}