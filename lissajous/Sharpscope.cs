using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using lissajous.Materials;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;

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
        public static int GetWidth
        {
            get { if (_instance != null) return _instance.Width; return 0; }
        }
        public static int GetHeight
        {
            get { if (_instance != null) return _instance.Height; return 0; }
        }
        public static void Blit (Texture source, Texture destination, bool clear = true)
        {
            _instance.blitMat.Use(source, destination);
            _instance.blitMat.Render(source, destination, clear);
        }
        internal static Sharpscope _instance;

        private readonly float[] quad = new float[]
        {
            -1.0f, -1.0f,
            1.0f, -1.0f,
            -1.0f,  1.0f,
            -1.0f,  1.0f,
            1.0f, -1.0f,
            1.0f,  1.0f
        };

        private Queue<VertexData> dataQueue = new Queue<VertexData>();
        private float[] vertices = new float[] {};
        private int[] indices = new int[] {};

        private int vertexLength = 2000;
        private int ticks = 0;

        private int quadArrayObject;
        private int quadVertexObject;
        private int frameBufferObject;

        private int vertexBufferObject;
        private int elementBufferObject;
        private int vertexArrayObject;

        public static Texture ScreenGrab;
        private Texture ppBufferA;
        private Texture ppBufferB;

        private List<Material> ppList;  // list of post processing shaders
        private LineMaterial lineMat;
        private ThresholdMaterial threshMat;
        private BlurMaterial blurMat;
        private CombineMaterial combineMat;
        private Material blitMat;

        private Settings settings;
        private SettingsWindow settingsWindow;
        private WasapiLoopbackCapture audio;

        private Random random;

        public Sharpscope (int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        protected override void OnLoad(EventArgs e)
        {
            if (_instance == null || _instance != this) _instance = this;

            random = new Random((int)DateTime.Now.Ticks);

            // load settings
            settings = new Settings();
            settings.PropertyChanged += OnSettingsChanged;

            loadFrameBuffer();
            loadGL();
            loadNAudio();

            base.OnLoad(e);
        }

        private void OnSettingsChanged (object sender, SettingsEventArgs e)
        {
            switch(e.Type)
            {
                case PropertyType.LINE_WIDTH:
                    LineTools.Width = settings.LineWidth;
                    break;
                case PropertyType.LINE_COLOR:
                    lineMat.Color = settings.LineColorVec();
                    break;
                case PropertyType.LINE_ALPHA:
                    lineMat.AlphaLength = settings.AlphaLength;
                    lineMat.MaxAlpha = settings.MaxAlpha;
                    lineMat.MinAlpha = settings.MinAlpha;
                    break;
                case PropertyType.GLOW:
                    threshMat.Glow = settings.Glow;
                    break;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if(e.Button == MouseButton.Right && e.Mouse.RightButton == ButtonState.Released)
            {
                if(settingsWindow == null)
                {
                    settingsWindow = new SettingsWindow(settings);
                    settingsWindow.FormClosing += OnSettingsClosing;
                    settingsWindow.Show();
                }
                else
                {
                    settingsWindow.BringToFront();
                }
            }
        }
        
        private void OnSettingsClosing (object sender, EventArgs e)
        {
            // save settings
            settingsWindow.FormClosing -= OnSettingsClosing;
            settingsWindow = null;
        }

        protected override void OnUnload(EventArgs e)
        {
            unloadGL();
            unloadNAudio();

            base.OnUnload(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObject);   // set off-screen buffer as target

            lineMat.Use(null, ppBufferA);
            DrawLines();  // render lines to buffer A

            // -------- start post-processing

            GL.BindVertexArray(quadArrayObject);
            Blit(ppBufferA, ScreenGrab); // store current screen

            Texture tmp;
            // render each pp shader
            for(int i = 0; i < ppList.Count; i++)
            {
                if (!ppList[i].IsEnabled) continue;

                ppList[i].Use(ppBufferA, ppBufferB);
                // switch buffers
                tmp = ppBufferA;
                ppBufferA = ppBufferB;
                ppBufferB = tmp;
            }

            // render last shader to screen
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            Blit(ppBufferA, null);  // blit to screen buffer

            SwapBuffers(); // present
            ticks++;

            base.OnRenderFrame(e);
        }

        private void DrawLines ()
        {
            // calculate and setup vertex data
            int vCount = Math.Min(vertexLength, dataQueue.Count);   // get vertex count
            VertexData vd;

            for (int i = 0; i < vCount; i++)
            {
                int idx = i * LineMaterial.VERTEX_STRIDE;
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

            // bind, copy and render the data

            GL.BindVertexArray(vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vCount * LineMaterial.VERTEX_STRIDE * sizeof(float), vertices, BufferUsageHint.StreamDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, vCount * 2 * sizeof(uint), indices, BufferUsageHint.StreamDraw);

            GL.DrawElements(PrimitiveType.TriangleStrip, vCount * 2, DrawElementsType.UnsignedInt, 0);
        }

        protected override void OnResize(EventArgs e)
        {
            //////  update materials / textures

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

            if (settings.Interpolate)
                data = LineTools.Interpolate(data, settings.IntrpLevel);

            LineTools.ComputeNormals(ref data);
            if (dataQueue.Count > data.Count) dataQueue.Clear();
            ticks = Math.Max(ticks - 1, 1);
            vertexLength = (int)Math.Ceiling((double)(data.Count / ticks));
            /*vertexLength = Math.Max(vertexLength, 1000);
            vertexLength = Math.Min(vertexLength, data.Count);*/
            ticks = 0;
            data.ForEach(dataQueue.Enqueue);

            // if the new vertex count is bigger than the cache, set it up again
            if (vertexLength * LineMaterial.VERTEX_STRIDE > vertices.Length) setupArrays(vertexLength);
        }

        private void setupArrays (int length)
        {
            vertices = new float[length * LineMaterial.VERTEX_STRIDE];
            indices = new int[length * 2];
            for (int i = 0; i < indices.Length; i++) indices[i] = i;
        }

        private void loadFrameBuffer ()
        {
            frameBufferObject = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObject);
            GL.DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });

            ScreenGrab = new Texture(Width, Height);
            ppBufferA = new Texture(Width, Height);
            ppBufferB = new Texture(Width, Height);

            quadVertexObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVertexObject);
            GL.BufferData(BufferTarget.ArrayBuffer, quad.Length * sizeof(float), quad, BufferUsageHint.StaticDraw);

            quadArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(quadArrayObject);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // set up post-proc material list

            ppList = new List<Material>();

            threshMat = new ThresholdMaterial();
            ppList.Add(threshMat);

            blurMat = new BlurMaterial();
            ppList.Add(blurMat);

            combineMat = new CombineMaterial();
            ppList.Add(combineMat);

            blitMat = new Material();
        }

        private void loadGL ()
        {
            setupArrays(vertexLength);

            GL.ClearColor(0f, 0f, 0f, 1f);

            GL.Enable(EnableCap.FramebufferSrgb);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.DstAlpha);
            // GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
            //GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);

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

            lineMat = new LineMaterial();
        }

        private void unloadGL ()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteFramebuffer(frameBufferObject);
            GL.DeleteBuffer(quadVertexObject);
            GL.DeleteVertexArray(quadArrayObject);
            GL.DeleteBuffer(vertexBufferObject);
            GL.DeleteBuffer(elementBufferObject);
            GL.DeleteVertexArray(vertexArrayObject);
            lineMat.Dispose();
            blitMat.Dispose();
            threshMat.Dispose();
            blurMat.Dispose();
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
            audio.DataAvailable -= Audio_DataAvailable;
            audio.Dispose();
        }
    }
}
