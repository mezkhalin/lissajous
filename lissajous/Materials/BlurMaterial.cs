using OpenTK.Graphics.OpenGL4;
using System;

namespace lissajous.Materials
{
    public class BlurMaterial : Material
    {
        public bool Horizontal = true;

        private Texture temp;

        public BlurMaterial()
        {
            RenderShader = new Shader("Shaders/quad.vert", "Shaders/gauss.frag");
            temp = new Texture(Width, Height);
        }

        public override void Use(Texture source, Texture target)
        {
            RenderShader.Use();

            RenderShader.SetInt("Horizontal", 1);
            Render(source, temp);
            RenderShader.SetInt("Horizontal", 0);
            Render(temp, target);
        }

        public override void Dispose()
        {
            base.Dispose();
            GL.DeleteTexture(temp.Handle);
        }
    }
}
