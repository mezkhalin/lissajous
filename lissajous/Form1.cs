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

    public partial class Form1 : Form
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
        public static float Fade = .5f;
        public static int GStrength = 40;
        public static int RStrength = 15;
        public static int BStrength = 15;

        public Point[] Buffer = new Point[] { new Point(0f, 0f) };
        public int BufferSize = 1;
        
        private Bitmap bitmap;
        private BitmapData bitmapData;
        private byte[] bitmapBytes;

        public Form1()
        {
            InitializeComponent();
            
            bitmap = new Bitmap(BitmapWidth, BitmapHeight, PixelFormat.Format24bppRgb);
            LockBitmap(false);
            UnlockBitmap();

            Application.Idle += Application_Idle;
            Paint += OnPaint;

            WasapiLoopbackCapture capture = new WasapiLoopbackCapture();
            capture.DataAvailable += Capture_DataAvailable;
            capture.RecordingStopped += (s, e) => { capture.Dispose(); };
            FormClosing += (s, e) => { capture.StopRecording(); };

            capture.StartRecording();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bitmap, ClientRectangle);
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

        private void Application_Idle(object sender, EventArgs e)
        {
            while(IsIdle())
            {
                if (BufferSize > 1)
                {
                    LockBitmap();

                    for (int i = 0; i < bitmapBytes.Length; i++)
                        bitmapBytes[i] = (byte)(bitmapBytes[i] * Fade);

                    PaintBuffer();

                    UnlockBitmap();

                    Refresh();
                }
            }
        }

        private void PaintBuffer ()
        {
            int x, y, pos;
            for(int i = 0; i < BufferSize; i++)
            {
                x = (int)(Buffer[i].X * (BitmapWidth - 1));
                y = (int)(Buffer[i].Y * (BitmapHeight - 1));
                pos = (y * bitmapData.Stride) + (x * 3);
                bitmapBytes[pos + 1] = (byte)Math.Min(bitmapBytes[pos + 1] + GStrength, 255);
                bitmapBytes[pos] = (byte)Math.Min(bitmapBytes[pos] + BStrength, 255);
                bitmapBytes[pos + 2] = (byte)Math.Min(bitmapBytes[pos + 2] + RStrength, 255);
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
