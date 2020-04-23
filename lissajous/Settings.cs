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
        LINE_COLOR,
        LINE_ALPHA,
        GLOW
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
        private float _lineWidth = 0.009f;

        public Vector3 LineColorVec () { return _lineColor; }
        public Color LineColor
        {
            get { return Color.FromArgb(
                (int)(_lineColor.X * byte.MaxValue),
                (int)(_lineColor.Y * byte.MaxValue),
                (int)(_lineColor.Z * byte.MaxValue)
            ); }
            set {
                _lineColor.X = value.R / (float)byte.MaxValue;
                _lineColor.Y = value.G / (float)byte.MaxValue;
                _lineColor.Z = value.B / (float)byte.MaxValue;
                OnPropertyChanged(PropertyType.LINE_COLOR);
            }
        }
        private Vector3 _lineColor = new Vector3(.1f, 1f, .1f);

        public bool Interpolate { get; set; } = false;
        public int IntrpLevel { get; set; } = 5;

        public float AlphaLength
        {
            get { return _alphaLength; }
            set
            {
                _alphaLength = value;
                OnPropertyChanged(PropertyType.LINE_ALPHA);
            }
        }
        private float _alphaLength = .022f;

        public float MinAlpha
        {
            get { return _minAlpha; }
            set
            {
                _minAlpha = value;
                OnPropertyChanged(PropertyType.LINE_ALPHA);
            }
        }
        private float _minAlpha = .006f;

        public float MaxAlpha
        {
            get { return _maxAlpha; }
            set
            {
                _maxAlpha = value;
                OnPropertyChanged(PropertyType.LINE_ALPHA);
            }
        }
        private float _maxAlpha = .06f;

        public float Glow
        {
            get { return _glow; }
            set
            {
                _glow = value;
                OnPropertyChanged(PropertyType.GLOW);
            }
        }
        private float _glow = 0f;

        public Settings ()
        {

        }

        private void OnPropertyChanged (PropertyType type)
        {
            PropertyChanged?.Invoke(this, new SettingsEventArgs(type));
        }
    }
}
