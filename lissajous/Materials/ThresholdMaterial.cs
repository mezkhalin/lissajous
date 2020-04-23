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
        public float Glow = 0f;

        public ThresholdMaterial ()
        {
            RenderShader = new Shader("Shaders/quad.vert", "Shaders/thresh.frag");
        }

        public override void Use(Texture source, Texture target)
        {
            RenderShader.SetFloat("Glow", Glow);
            RenderShader.Use();
            Render(source, target);
        }
    }
}
