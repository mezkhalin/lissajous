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
        public static float Width = .0085f;

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
    }
}
