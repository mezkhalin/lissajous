using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lissajous.Shaders;

namespace lissajous
{
    public struct VertexData
    {
        public Vector2 Position;
        public Vector2 Normal;
        public float MiterLength;

        public float X;
        public float Y;
        public float nX;
        public float nY;
        public float nL;
    }

    public class Sharpscope : GameWindow
    {
        public static readonly int ATTRIB_COUNT = 7;
        public static readonly int VERTEX_STRIDE = ATTRIB_COUNT * 2;

        private float[] testSquare = new float[]
        {
            -.5f, -.5f,
            -.5f, .48f,
            .5f, .5f,
            -.5f, .5f
        };

        private Queue<VertexData> dataQueue = new Queue<VertexData>();

        private float[] vertices = new float[]
        {
            
        };

        private int[] indices = new int[]
        {
            
        };

        private int vertexLength = 2000;
        private int ticks = 0;

        private int vertexBufferObject;
        private int elementBufferObject;
        private int vertexArrayObject;

        private Shader shader;

        private WasapiLoopbackCapture audio;

        private Random random;

        public Sharpscope (int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        protected override void OnLoad(EventArgs e)
        {
            random = new Random((int)DateTime.Now.Ticks);

            loadGL();
            loadNAudio();

            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            unloadGL();
            unloadNAudio();

            base.OnUnload(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            int vCount = Math.Min(vertexLength, dataQueue.Count);   // get vertex count
            VertexData vd;

            for (int i = 0; i < vCount; i++)
            {
                int idx = i * VERTEX_STRIDE;
                vd = dataQueue.Dequeue();

                vertices[idx] = vertices[idx + 7] = vd.Position.X;
                vertices[idx + 1] = vertices[idx + 8] = vd.Position.Y;

                if (i < vCount - 1)  // positions for next point in line
                {
                    vertices[idx + 2] = vertices[idx + 9] = dataQueue.Peek().Position.X;
                    vertices[idx + 3] = vertices[idx + 10] = dataQueue.Peek().Position.Y;
                }
                else
                {
                    vertices[idx + 2] = vertices[idx + 9] = vd.Position.X;
                    vertices[idx + 3] = vertices[idx + 10] = vd.Position.Y;
                }

                vertices[idx + 4] = vd.Normal.X;
                vertices[idx + 5] = vd.Normal.Y;
                vertices[idx + 11] = vd.Normal.X * -1f;
                vertices[idx + 12] = vd.Normal.Y * -1f;
                vertices[idx + 6] = vertices[idx + 13] = vd.MiterLength;
            }

            //--------

            GL.Clear(ClearBufferMask.ColorBufferBit);

            shader.Use();

            GL.BindVertexArray(vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vCount * VERTEX_STRIDE * sizeof(float), vertices, BufferUsageHint.StreamDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, vCount * 2 * sizeof(uint), indices, BufferUsageHint.StreamDraw);
            
            GL.DrawElements(PrimitiveType.TriangleStrip, vCount * 2, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();

            ticks++;

            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        private void Audio_DataAvailable(object sender, WaveInEventArgs e)
        {
            VertexData vd;
            List<VertexData> data = new List<VertexData>();

            for(int i = 0; i < e.BytesRecorded; i+=8)
            {
                vd = new VertexData();
                vd.Position = new Vector2(
                        BitConverter.ToSingle(e.Buffer, i),
                        BitConverter.ToSingle(e.Buffer, i + 4)
                    );
                data.Add(vd);
            }

            if (dataQueue == null) return;  // test fix for error on close

            LineTools.ComputeNormals(ref data);
            if (dataQueue.Count >= data.Count) dataQueue.Clear();
            ticks = Math.Max(ticks - 1, 1);
            vertexLength = (int)Math.Ceiling((double)(data.Count / ticks));
            ticks = 0;
            data.ForEach(dataQueue.Enqueue);

            if (vertexLength * VERTEX_STRIDE > vertices.Length) setupArrays(vertexLength);
        }

        private void setupArrays (int length)
        {
            vertices = new float[length * VERTEX_STRIDE];
            indices = new int[length * 2];
            for (int i = 0; i < indices.Length; i++) indices[i] = i;
        }

        private void loadGL ()
        {
            setupArrays(vertexLength);

            GL.ClearColor(0f, 0f, 0f, 1f);
            
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.DstAlpha);

            shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            shader.Use();

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StreamDraw);

            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StreamDraw);

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            int posLocation = shader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(posLocation, 2, VertexAttribPointerType.Float, false, ATTRIB_COUNT * sizeof(float), 0);
            GL.EnableVertexAttribArray(posLocation);

            int nxtLocation = shader.GetAttribLocation("aNext");
            GL.VertexAttribPointer(nxtLocation, 2, VertexAttribPointerType.Float, false, ATTRIB_COUNT * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(nxtLocation);

            int norLocation = shader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(norLocation, 3, VertexAttribPointerType.Float, true, ATTRIB_COUNT * sizeof(float), 4 * sizeof(float));
            GL.EnableVertexAttribArray(norLocation);
        }

        private void unloadGL ()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(vertexBufferObject);
            GL.DeleteBuffer(elementBufferObject);
            GL.DeleteVertexArray(vertexArrayObject);
            GL.DeleteProgram(shader.Handle);
        }

        private void loadNAudio ()
        {
            audio = new WasapiLoopbackCapture();
            audio.DataAvailable += Audio_DataAvailable;
            audio.StartRecording();
        }

        private void unloadNAudio ()
        {
            audio.StopRecording();
            audio.Dispose();
        }
    }
}
