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
            
            lineColorButton.BackColor = Context.LineColor;

            lineSmoothCheckbox.Checked = Context.Interpolate;
            lineSmoothLevel.Value = Context.IntrpLevel;

            intensityBar.Value = (int)(Context.AlphaLength * 1000f);
            alphaMaxBar.Value = (int)(Context.MaxAlpha * 1000f);
            alphaMinBar.Value = (int)(Context.MinAlpha * 1000f);

            glowBar.Value = (int)((2f - Context.Glow) * 100f);
        }

        private void Glow_SliderChanged (object sender, EventArgs e)
        {
            Context.Glow = 2f - (glowBar.Value / 100f);
            glowLabel.Text = (glowBar.Value / 100f).ToString();
        }

        private void Intensity_SliderChanged(object sender, EventArgs e)
        {
            Context.AlphaLength = intensityBar.Value / 1000f;
            intensityUpDown.Value = (decimal)Context.AlphaLength;

            Context.MaxAlpha = alphaMaxBar.Value / 1000f;
            alphaMaxUpDown.Value = (decimal)Context.MaxAlpha;

            Context.MinAlpha = alphaMinBar.Value / 1000f;
            alphaMinUpDown.Value = (decimal)Context.MinAlpha;
        }

        private void Intensity_UpDownChanged (object sender, EventArgs e)
        {
            Context.AlphaLength = (float)intensityUpDown.Value;
            intensityBar.Value = (int)(Context.AlphaLength * 1000f);

            Context.MaxAlpha = (float)alphaMaxUpDown.Value;
            alphaMaxBar.Value = (int)(Context.MaxAlpha * 1000f);

            Context.MinAlpha = (float)alphaMinUpDown.Value;
            alphaMinBar.Value = (int)(Context.MinAlpha * 1000f);
        }

        private void LineSmoothLevel_ValueChanged(object sender, EventArgs e)
        {
            Context.IntrpLevel = (int)lineSmoothLevel.Value;
        }

        private void LineSmoothCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Context.Interpolate = lineSmoothCheckbox.Checked;
        }

        private void LineColorButton_Click(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            dialog.Color = Context.LineColor;

            if (dialog.ShowDialog() == DialogResult.OK)
                Context.LineColor = lineColorButton.BackColor = dialog.Color;
        }

        private void LineWidthBar_ValueChanged(object sender, EventArgs e)
        {
            Context.LineWidth = lineWidthBar.Value / 10000f;
            lineWidthLabel.Text = Context.LineWidth.ToString();
        }
    }
}
