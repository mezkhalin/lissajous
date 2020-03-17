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
    public class Sharpscope : GameWindow
    {
        private float[] _v = new float[]
           {
            0.5f,  0.5f, 0.0f, // top right
             0.5f, -0.5f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, // top left
           };
        private float[] vertices = new float[]
        {
            0.5f,  0.5f, 0.0f, // top right
             0.5f, -0.5f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, // top left
        };

        private int[] indices = new int[0];

        private int vertexBufferObject;
        private int vertexArrayObject;
        private int elementBufferObject;

        private Shader shader;

        private WasapiLoopbackCapture audio;
        private bool invalid = true;
        private int indiceLength = 0;

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
            GL.Clear(ClearBufferMask.ColorBufferBit);

            shader.Use();

            GL.BindVertexArray(vertexArrayObject);

            if(invalid)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StreamDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indiceLength * sizeof(uint), indices, BufferUsageHint.StreamDraw);
                invalid = false;
            }
            
            GL.DrawElements(PrimitiveType.LineLoop, indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();

            base.OnRenderFrame(e);
        }

        private void Audio_DataAvailable(object sender, WaveInEventArgs e)
        {
            int numPoints = e.BytesRecorded / 8;
            vertices = new float[numPoints * 3];

            for (int i = 0; i < e.BytesRecorded; i += 8)
            {
                int pos = (i / 8) * 3;
                vertices[pos]       = BitConverter.ToSingle(e.Buffer, i);
                vertices[pos + 1]   = BitConverter.ToSingle(e.Buffer, i + 4);
            }

            if(numPoints > indices.Length)
            {
                indices = new int[numPoints];
                for (int i = 0; i < numPoints; i++) indices[i] = i;
            }
            indiceLength = numPoints;

            invalid = true;
        }

        private void loadGL ()
        {
            GL.ClearColor(0f, 0f, 0f, 1f);

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StreamDraw);

            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StreamDraw);

            shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            shader.Use();

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
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
