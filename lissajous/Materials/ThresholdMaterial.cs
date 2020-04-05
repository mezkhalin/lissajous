using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace lissajous.Materials
{
    public class ThresholdMaterial : Material
    {

        public ThresholdMaterial (int Width, int Height) : base(Width, Height)
        {
            RenderShader = new Shader("Shaders/quad.vert", "Shaders/thresh.frag");
            RenderShader.Use();
        }

        public override void Use(Texture source = null)
        {
            base.Use(source);
            UseRenderTarget();
        }
    }
}
