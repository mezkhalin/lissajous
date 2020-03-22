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
        public static float Width = .007f;

        public static void ComputeNormals (ref List<VertexData> data)
        {
            float doubleWidth = Width * 2f;
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

                dirA = Direction(cur.Position, last.Position);
                if (isFirst) // set up first normal
                {
                    last.Normal = Normal(dirA);
                    last.MiterLength = Width;
                    data[0] = last;
                    isFirst = false;
                }

                if(isLast)  // set up last normal
                {
                    cur.Normal = Normal(dirA);
                    cur.MiterLength = Width;
                    //data[i] = cur;
                }
                else
                {
                    dirB = Direction(next.Position, cur.Position);

                    // compute miter by Matt DesLauriers @mattdesl

                    // get tangent line
                    tangent = (dirA + dirB).Normalized();
                    // get miter as unit vector
                    cur.Normal = new Vector2(-tangent.Y, tangent.X);

                    //get the necessary length of our miter
                    cur.MiterLength = halfWidth / Vector2.Dot(cur.Normal, new Vector2(-dirA.Y, dirA.X));
                    if (cur.MiterLength < 0) cur.MiterLength = Math.Abs(cur.MiterLength);
                    if (cur.MiterLength > doubleWidth) cur.MiterLength = doubleWidth;
                }

                data[i] = cur;
            }
        }

        private static Vector3 ComputeMiter (Vector2 dirA, Vector2 dirB, float halfThickness)
        {
            // get tangent line
            Vector2 tangent = (dirA + dirB).Normalized();
            // get miter as unit vector
            Vector2 miter = new Vector2(-tangent.Y, tangent.X);
            Vector2 dirAN = new Vector2(-dirA.Y, dirA.X);

            //get the necessary length of our miter
            float length = halfThickness / Vector2.Dot(miter, dirAN);
            return new Vector3(miter.X, miter.Y, length);
        }

        /*private static Vector3[] ComputeNormals(Vector2[] points, bool closed = false)
        {

            for (int i = 1; i < points.Length; i++)
            {
                Vector2 last = points[i - 1];
                Vector2 cur = points[i];
                Vector2 next = Vector2.Zero;

                if (i < points.Length - 1) next = points[i + 1];
                else isLast = true;

                lineA = Direction(cur, last);
                if (isFirst)
                {
                    curNormal = Normal(lineA);
                    isFirst = false;
                }

                if (i == 1)   // addNext(curNomarl, 1)
                {
                    data.Add(new Vector3(curNormal.X, curNormal.Y, Width));
                }

                if (isLast)
                {
                    curNormal = Normal(lineA);
                    data.Add(new Vector3(curNormal.X, curNormal.Y, Width));
                }
                else
                {
                    lineB = Direction(next, cur);
                    float miterLength = ComputeMiter(lineA, lineB, Width, out miter);
                    if (miterLength < 0) miterLength = Math.Abs(miterLength);
                    miterLength = Math.Min(miterLength, Width * 2f);
                    data.Add(new Vector3(miter.X, miter.Y, miterLength));
                }
            }

            return data.ToArray();
        }*/

        /*public static List<VertexData> CreateLine (float[] pos)
        {
            Vector2[] points = new Vector2[pos.Length / 2];

            for(int i = 0; i < pos.Length; i+=2)
            {
                points[i / 2] = new Vector2(pos[i], pos[i + 1]);
            }

            Vector3[] normals = ComputeNormals(points);
            List<VertexData> data = new List<VertexData>();

            for(int i = 0; i < points.Length; i++)
            {
                Vector2 point = points[i];
                Vector3 normal = normals[i];
                VertexData d = new VertexData(
                        point.X, point.Y,
                        normal.X, normal.Y, normal.Z
                    );
                data.Add(d);
                d.nX *= -1f;
                d.nY *= -1f;
                data.Add(d);
            }

            return data;
        }*/

        #region UTILS

        private static Vector2 Normal (Vector2 dir)
        {
            return new Vector2(-dir.Y, dir.X);
        }

        private static Vector2 Direction (Vector2 a, Vector2 b)
        {
            return (a - b).Normalized();
        }

        ///------------------------------------------------------------///
        /// Miter function and line tool by Matt DesLauriers @mattdesl ///
        ///------------------------------------------------------------///

        private static Vector2 tangent = Vector2.Zero;

        private static float ComputeMiter (Vector2 lineA, Vector2 lineB, float halfThick, out Vector2 miter)
        {
            //get tangent line
            tangent = lineA + lineB;
            tangent.Normalize();

            //get miter as a unit vector
            miter = new Vector2(-tangent.Y, tangent.X);
            Vector2 tmp = new Vector2(-lineA.Y, lineA.X);

            //get the necessary length of our miter
            return halfThick / Vector2.Dot(miter, tmp);
        }

        #endregion
    }
}
