using OpenTK.Graphics.OpenGL4;
using System;

namespace lissajous.Materials
{
    public class BlurMaterial : Material
    {
        public bool Horizontal = true;
        public float StandardDeviation = .02f;
        public float StdDevSqrd;
        public float PiFact;

        public BlurMaterial(int Width, int Height) : base(Width, Height)
        {
            RenderShader = new Shader("Shaders/quad.vert", "Shaders/blur.frag");
            RenderShader.Use();
            SetStandardDeviation(20f);
        }

        public void SetStandardDeviation (float val)
        {
            StdDevSqrd = val * val;
            PiFact = 1f / (float)Math.Sqrt(2f * Math.PI * StdDevSqrd);
        }

        public override void Use(Texture source = null)
        {
            UseRenderTarget();
            RenderShader.SetFloat("StdDevSqrd", StdDevSqrd);
            RenderShader.SetFloat("PiFact", PiFact);
            RenderShader.SetInt("Horizontal", Horizontal ? 1 : 0);
            base.Use(source);
        }
    }
}
