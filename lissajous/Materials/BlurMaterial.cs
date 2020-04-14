using OpenTK.Graphics.OpenGL4;
using System;

namespace lissajous.Materials
{
    public class BlurMaterial : Material
    {
        public bool Horizontal = true;

        private Texture tempA;
        private Texture tempB;

        public BlurMaterial()
        {
            int fac = 8;
            RenderShader = new Shader("Shaders/quad.vert", "Shaders/gauss.frag");
            tempA = new Texture(Width / fac, Height / fac);
            tempB = new Texture(Width / fac, Height / fac);
        }

        public override void Use(Texture source, Texture target)
        {
            Sharpscope.Blit(source, tempA);     // downscale original

            RenderShader.Use();
            RenderShader.SetVector2("texelSize", new OpenTK.Vector2(1f / tempB.Width, 1f / tempB.Height));

            RenderShader.SetInt("Horizontal", 1);
            Render(tempA, tempB);
            RenderShader.SetInt("Horizontal", 0);
            Render(tempB, target);
        }

        public override void Dispose()
        {
            base.Dispose();
            GL.DeleteTexture(tempA.Handle);
            GL.DeleteTexture(tempB.Handle);
        }
    }
}
