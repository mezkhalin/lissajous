using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace lissajous.Materials
{
    public class LineMaterial : Material
    {
        public static readonly int ATTRIB_COUNT = 7;
        public static readonly int VERTEX_STRIDE = ATTRIB_COUNT * 2;

        public Vector3 Color = new Vector3(.2f, 1f, .2f);

        public LineMaterial()
        {
            RenderShader = new Shader("Shaders/line.vert", "Shaders/line.frag");

            int posLocation = RenderShader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(posLocation, 2, VertexAttribPointerType.Float, false, ATTRIB_COUNT * sizeof(float), 0);
            GL.EnableVertexAttribArray(posLocation);

            int nxtLocation = RenderShader.GetAttribLocation("aNext");
            GL.VertexAttribPointer(nxtLocation, 2, VertexAttribPointerType.Float, false, ATTRIB_COUNT * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(nxtLocation);

            int norLocation = RenderShader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(norLocation, 3, VertexAttribPointerType.Float, true, ATTRIB_COUNT * sizeof(float), 4 * sizeof(float));
            GL.EnableVertexAttribArray(norLocation);
        }

        public override void Use(Texture source, Texture target)
        {
            RenderShader.SetVector3("Color", Color);
            RenderShader.Use();
            Render(source, target, true, false);
        }
    }
}
