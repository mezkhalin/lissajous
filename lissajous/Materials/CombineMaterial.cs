using OpenTK.Graphics.OpenGL4;

namespace lissajous.Materials
{
    public class CombineMaterial : Material
    {
        public Texture Combine;

        public CombineMaterial (int Width, int Height) : base(Width, Height)
        {
            RenderShader = new Shader("Shaders/quad.vert", "Shaders/combine.frag");
            RenderShader.Use();
        }

        public override void Use(Texture source = null)
        {
            base.Use(source);
            RenderShader.SetInt("Combine", 1);
            Combine.Use(TextureUnit.Texture1);
        }
    }
}
