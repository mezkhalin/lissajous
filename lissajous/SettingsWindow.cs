using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lissajous
{
    public partial class SettingsWindow : Form
    {
        private Settings Context;

        public SettingsWindow(Settings context)
        {
            InitializeComponent();

            Context = context;

            lineWidthBar.Value = (int)(Context.LineWidth * 10000f);
            lineWidthLabel.Text = Context.LineWidth.ToString();
            
            lineColorButton.BackColor = Context.LineColor;
            lineColorButton.Click += LineColorButton_Click;
        }

        private void LineColorButton_Click(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            dialog.Color = Context.LineColor;

            if (dialog.ShowDialog() == DialogResult.OK)
                Context.LineColor = lineColorButton.BackColor = dialog.Color;
        }

        private void lineWidthBar_ValueChanged(object sender, EventArgs e)
        {
            Context.LineWidth = lineWidthBar.Value / 10000f;
            lineWidthLabel.Text = Context.LineWidth.ToString();
        }
    }
}
