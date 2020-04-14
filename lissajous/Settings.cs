using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace lissajous
{
    public enum PropertyType
    {
        LINE_WIDTH,
        LINE_COLOR
    }

    public class SettingsEventArgs : EventArgs
    {
        public PropertyType Type { get; set; }

        public SettingsEventArgs (PropertyType type)
        {
            Type = type;
        }
    }

    public class Settings
    {
        public EventHandler<SettingsEventArgs> PropertyChanged;

        public float LineWidth
        {
            get { return _lineWidth; }
            set { _lineWidth = value; OnPropertyChanged(PropertyType.LINE_WIDTH); }
        }
        private float _lineWidth = 0.008f;

        public Vector3 LineColorVec () { return _lineColor; }
        public Color LineColor
        {
            get { return Color.FromArgb(
                (int)(_lineColor.X * byte.MaxValue),
                (int)(_lineColor.Y * byte.MaxValue),
                (int)(_lineColor.Z * byte.MaxValue)
            ); }
            set {
                _lineColor.X = value.R / byte.MaxValue;
                _lineColor.Y = value.G / byte.MaxValue;
                _lineColor.Z = value.B / byte.MaxValue;
                OnPropertyChanged(PropertyType.LINE_COLOR);
            }
        }
        private Vector3 _lineColor = new Vector3(.2f, 1f, .2f);

        public bool Interpolate { get; set; } = true;
        public int IntrpLevel { get; set; } = 1;

        public Settings ()
        {

        }

        private void OnPropertyChanged (PropertyType type)
        {
            PropertyChanged?.Invoke(this, new SettingsEventArgs(type));
        }
    }
}
