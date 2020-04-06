using OpenTK.Graphics.OpenGL4;

namespace lissajous.Materials
{
    public class CombineMaterial : Material
    {
        public Texture Combine;

        public CombineMaterial ()
        {
            RenderShader = new Shader("Shaders/quad.vert", "Shaders/combine.frag");
            RenderShader.Use();
            Combine = Sharpscope.ScreenGrab;
        }

        public override void Use(Texture source, Texture target)
        {
            RenderShader.SetInt("Combine", 1);
            Combine.Use(TextureUnit.Texture1);
            Render(source, target);
        }
    }
}
