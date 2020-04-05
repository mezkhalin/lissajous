using OpenTK.Graphics.OpenGL4;

namespace lissajous.Materials
{
    public class Material
    {
        public Texture Source;
        public Texture RenderTarget;
        public Shader RenderShader;

        internal int Width;
        internal int Height;

        public Material (int Width, int Height)
        {
            this.Height = Height;
            this.Width = Width;

            RenderTarget = new Texture(Width, Height);
            RenderShader = new Shader("Shaders/quad.vert", "Shaders/pass.frag");
        }

        public virtual void Use(Texture source = null)
        {
            if (RenderShader == null) return;

            if (source != null)
            {
                Source = source;
                RenderShader.SetInt("Source", 0);
                Source.Use(TextureUnit.Texture0);
            }

            RenderShader.Use();
        }

        internal void UseRenderTarget (bool clear = true)
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, RenderTarget.Handle, 0);
            GL.Viewport(0, 0, Width, Height);
            if(clear) GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public virtual void Dispose ()
        {
            if(Source != null) GL.DeleteTexture(Source.Handle);
            if(RenderTarget != null) GL.DeleteTexture(RenderTarget.Handle);
            if(RenderShader != null) GL.DeleteProgram(RenderShader.Handle);
        }
    }
}
