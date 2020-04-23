using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lissajous
{

    public static class LineTools
    {
        public static float Width = .009f;

        public static void ComputeNormals (ref List<VertexData> data)
        {
            float miterMax = Width * 1.5f;
            float halfWidth = Width / 2f;
            bool isFirst = true;  // work-around non-nullable vectors
            bool isLast = false;
            VertexData last, cur, next = new VertexData();
            Vector2 tangent, dirA, dirB, curNormal = Vector2.Zero;

            for(int i = 1; i < data.Count; i++)
            {
                last = data[i - 1];
                cur = data[i];
                if (i < data.Count - 1) next = data[i + 1];
                else isLast = true;

                dirA = (cur.Position - last.Position).Normalized();
                if (isFirst) // set up first normal
                {
                    last.Normal = new Vector2(-dirA.Y, dirA.X);
                    last.MiterLength = Width;
                    data[0] = last;
                    isFirst = false;
                }

                if(isLast)  // set up last normal
                {
                    cur.Normal = new Vector2(-dirA.Y, dirA.X);
                    cur.MiterLength = Width;
                }
                else
                {
                    dirB = (next.Position - cur.Position).Normalized();

                    // compute miter by Matt DesLauriers @mattdesl

                    // get tangent line
                    tangent = (dirA + dirB).Normalized();
                    // get miter as unit vector
                    cur.Normal = new Vector2(-tangent.Y, tangent.X);

                    //get the necessary length of our miter
                    cur.MiterLength = halfWidth / Vector2.Dot(cur.Normal, new Vector2(-dirA.Y, dirA.X));
                    if (cur.MiterLength < -miterMax) cur.MiterLength = -miterMax; // Math.Abs(cur.MiterLength);
                    if (cur.MiterLength > miterMax) cur.MiterLength = miterMax;
                }

                data[i] = cur;
            }
        }

        public static List<VertexData> Interpolate (List<VertexData> data, int steps)
        {
            List<VertexData> rtn = new List<VertexData>();
            float muStep = 1f / (steps + 1f);
            Vector2 v0, v1, v2, v3;
            VertexData vd;

            for(int j = 0; j < data.Count - 1; j++)
            {
                rtn.Add(data[j]);

                v1 = data[j].Position;
                v2 = data[j + 1].Position;

                if (j == 0)
                    v0 = v1 - (v2 - v1);
                else
                    v0 = data[j - 1].Position;

                if (j == data.Count - 2)
                    v3 = v2 + (v2 - v1);
                else
                    v3 = data[j + 2].Position;

                for(int i = 0; i < steps; i++)
                {
                    float mu = muStep * (i + 1);
                    float mu2 = mu * mu;
                    vd = new VertexData();
                    vd.Position = new Vector2(
                            Cubic(v0.X, v1.X, v2.X, v3.X, mu, mu2),
                            Cubic(v0.Y, v1.Y, v2.Y, v3.Y, mu, mu2)
                        );
                    rtn.Add(vd);
                }
            }

            // dont forget the last data point
            if(data.Count > 1) rtn.Add(data[data.Count - 1]);

            return rtn;
        }

        private static float Cubic (float v0, float v1, float v2, float v3, float mu, float mu2)
        {
            float a0,a1,a2;
            
            a0 = v3 - v2 - v0 + v1;
            a1 = v0 - v1 - a0;
            a2 = v2 - v0;

            return(a0*mu*mu2+a1*mu2+a2*mu+v1);
        }
    }
}
