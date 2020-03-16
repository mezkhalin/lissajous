using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lissajous
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMessage
    {
        public IntPtr Handle;
        public uint Message;
        public IntPtr WParameter;
        public IntPtr LParameter;
        public uint Time;
        public Point Location;
    }

    public struct Point
    {
        public float X;
        public float Y;

        public Point(float x, float y)
        {
            X = x * .5f + .5f;
            Y = 1f - (y * .5f + .5f);
        }
    }

    public partial class Lissascoupe : Form
    {
        [DllImport("user32.dll")]
        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);
        public bool IsIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
        }

        public static readonly int BitmapWidth = 400;
        public static readonly int BitmapHeight = 400;

        public static float Fade = .6f;
        public static int GStrength = 15;
        public static int RStrength = 5;
        public static int BStrength = 5;

        public Point[] Buffer = new Point[] { new Point(0f, 0f) };
        public int BufferSize = 1;
        
        private Bitmap bitmap;
        private BitmapData bitmapData;
        private byte[] bitmapBytes;

        private int midByteIdx;

        public Lissascoupe()
        {
            InitializeComponent();
            
            bitmap = new Bitmap(BitmapWidth, BitmapHeight, PixelFormat.Format24bppRgb);
            LockBitmap(false);
            UnlockBitmap();

            midByteIdx = ((BitmapHeight / 2) * bitmapData.Stride) + ((BitmapWidth / 2) * 3);

            Application.Idle += Application_Idle;
            Paint += OnPaint;

            WasapiLoopbackCapture capture = new WasapiLoopbackCapture();
            capture.DataAvailable += Capture_DataAvailable;
            capture.RecordingStopped += (s, e) => { capture.Dispose(); };
            FormClosing += (s, e) => { capture.StopRecording(); };

            capture.StartRecording();
        }

        private void LockBitmap (bool copy = true)
        {
            bitmapData = bitmap.LockBits(new Rectangle(0, 0, BitmapWidth, BitmapHeight), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            
            int bytes = Math.Abs(bitmapData.Stride) * BitmapHeight;
            if(bitmapBytes == null) bitmapBytes = new byte[bytes];

            if(copy) Marshal.Copy(bitmapData.Scan0, bitmapBytes, 0, bytes);
        }

        private void UnlockBitmap ()
        {
            Marshal.Copy(bitmapBytes, 0, bitmapData.Scan0, Math.Abs(bitmapData.Stride) * BitmapHeight);
            bitmap.UnlockBits(bitmapData);
        }

        private void SetPixel (int x, int y)
        {
            int pos = (y * bitmapData.Stride) + (x * 3);
            bitmapBytes[pos] = (byte)Math.Min(bitmapBytes[pos] + BStrength, 255);
            bitmapBytes[pos + 1] = (byte)Math.Min(bitmapBytes[pos + 1] + GStrength, 255);
            bitmapBytes[pos + 2] = (byte)Math.Min(bitmapBytes[pos + 2] + RStrength, 255);
        }

        private void efla_d(int x, int y, int x2, int y2)
        {
            bool yLonger = false;
            int incrementVal, endVal;
            int shortLen = y2 - y;
            int longLen = x2 - x;

            if (Math.Abs(shortLen) > Math.Abs(longLen))
            {
                int swap = shortLen;
                shortLen = longLen;
                longLen = swap;
                yLonger = true;
            }

            endVal = longLen;

            if (longLen < 0)
            {
                incrementVal = -1;
                longLen = -longLen;
            }
            else incrementVal = 1;

            int decInc;
            if (longLen == 0) decInc = 0;
            else decInc = (shortLen << 16) / longLen;

            int j = 0;
            if (yLonger)
            {
                for (int i = 0; i != endVal; i += incrementVal)
                {
                    SetPixel(x + (j >> 16), y + i);
                    j += decInc;
                }
            }
            else
            {
                for (int i = 0; i != endVal; i += incrementVal)
                {
                    SetPixel(x + i, y + (j >> 16));
                    j += decInc;
                }
            }
        }

        private void testSquare (int x, int y, int x2, int y2)
        {
            efla_d(x, y, x2, y2);
            efla_d(x2, y2, x2 + (y - y2), y2 + (x2 - x));
            efla_d(x, y, x + (y - y2), y + (x2 - x));
            efla_d(x + (y - y2), y + (x2 - x), x2 + (y - y2), y2 + (x2 - x));
        }

        private void PaintBuffer ()
        {
            int w = BitmapWidth - 1;
            int h = BitmapHeight - 1;
            int x, y, x2, y2;
            Point p, p2;
            for(int i = 0; i < BufferSize - 1; i++)
            {
                p = Buffer[i];
                p2 = Buffer[i + 1];
                x = (int)(p.X * w);
                y = (int)(p.Y * h);
                x2 = (int)(p2.X * w);
                y2 = (int)(p2.Y * h);
                efla_d(x, y, x2, y2);
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bitmap, ClientRectangle);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            while (IsIdle())
            {

                if (BufferSize > 1)
                {
                    LockBitmap();

                    for (int i = 0; i < bitmapBytes.Length; i++)
                    {
                        float v = bitmapBytes[i] / 255f;
                        bitmapBytes[i] = (byte)(v * v * 255 * Fade);
                    }

                    PaintBuffer();

                    //Buffer = new Point[0];
                    //BufferSize = 0;

                    UnlockBitmap();
                    Refresh();
                }
            }
        }

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            Point[] buf = new Point[e.BytesRecorded / 8];

            for(int i = 0; i < e.BytesRecorded; i += 8)
            {
                float _l = BitConverter.ToSingle(e.Buffer, i);
                float _r = BitConverter.ToSingle(e.Buffer, i + 4);
                buf[i / 8] = new Point(_l, _r);
            }

            try
            {
                Invoke(new Action(() =>
                {
                    Buffer = buf;
                    BufferSize = buf.Length;
                }));
            }
            catch(ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
