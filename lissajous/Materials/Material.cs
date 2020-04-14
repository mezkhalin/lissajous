using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace lissajous.Materials
{
    public class Material
    {
        public Shader RenderShader;
        public bool IsEnabled = true;

        internal int Width { get { return Sharpscope.GetWidth; } }
        internal int Height { get { return Sharpscope.GetHeight; } }

        public Material ()
        {
            RenderShader = new Shader("Shaders/quad.vert", "Shaders/pass.frag");
        }

        public virtual void Use(Texture source, Texture target)
        {
            if (RenderShader == null) return;
            RenderShader.Use();
        }

        public void Render (Texture source, Texture target, bool clear = true, bool isPost = true)
        {
            if(source != null)
            {
                RenderShader.SetInt("Source", 0);
                source.Use(TextureUnit.Texture0);
            }

            if(target != null)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, target.Handle, 0);
                GL.Viewport(0, 0, target.Width, target.Height);
                if(clear) GL.Clear(ClearBufferMask.ColorBufferBit);
            }
            
            if (isPost) GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public virtual void Dispose ()
        {
            if(RenderShader != null) GL.DeleteProgram(RenderShader.Handle);
        }
    }
}
