using LightDx.InputAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial21
{
    struct ModelVertex
    {
        [Position]
        public Vector3 Position;
        [TexCoord]
        public Vector2 TexutreCoord;
        [Normal]
        public Vector3 Normal;
        [Tangent]
        public Vector3 Tangent;
        [Binormal]
        public Vector3 Binormal;
    }

    class Model
    {
        public static ModelVertex[] ReadModelFile(Stream stream)
        {
            StreamReader r = new StreamReader(stream);
            int ch;

            ch = r.Read();
            while (ch != ':')
            {
                ch = r.Read();
            }

            var n = Int32.Parse(r.ReadLine());
            var ret = new ModelVertex[n];

            ch = r.Read();
            while (ch != ':')
            {
                ch = r.Read();
            }
            r.ReadLine();

            for (int i = 0; i < n; ++i)
            {
                var line = r.ReadLine().Trim();
                if (line.Length == 0)
                {
                    i -= 1;
                    continue;
                }
                var v = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(num => Single.Parse(num)).ToArray();
                ret[i].Position.X = v[0];
                ret[i].Position.Y = v[1];
                ret[i].Position.Z = v[2];
                ret[i].TexutreCoord.X = v[3];
                ret[i].TexutreCoord.Y = v[4];
                ret[i].Normal.X = v[5];
                ret[i].Normal.Y = v[6];
                ret[i].Normal.Z = v[7];
            }

            for (int i = 0; i < n; i += 3)
            {
                CalculateVectors(ret, i);
            }

            return ret;
        }

        private static void CalculateVectors(ModelVertex[] vertex, int startIndex)
        {
            var vector1 = vertex[startIndex + 1].Position - vertex[startIndex].Position;
            var vector2 = vertex[startIndex + 2].Position - vertex[startIndex].Position;
            var tvector1 = vertex[startIndex + 1].TexutreCoord - vertex[startIndex].TexutreCoord;
            var tvector2 = vertex[startIndex + 2].TexutreCoord - vertex[startIndex].TexutreCoord;

            //tu[0]: tvec1.x
            //tv[0]: tvec1.y
            //tu[1]: tvec2.x
            //tv[1]: tvec2.y

            //Why do we need the denominator if we'll normalize?
            var den = 1 / (tvector1.X * tvector2.Y - tvector2.X * tvector1.Y);
            var t = (tvector2.Y * vector1 - tvector1.Y * vector2) * den;
            var b = (tvector1.X * vector2 - tvector2.X * vector1) * den;

            t = Vector3.Normalize(t);
            b = Vector3.Normalize(b);

            var n = Vector3.Cross(t, b);
            n = Vector3.Normalize(n);

            UpdateVectors(ref vertex[startIndex], t, b, n);
            UpdateVectors(ref vertex[startIndex + 1], t, b, n);
            UpdateVectors(ref vertex[startIndex + 2], t, b, n);
        }

        private static void UpdateVectors(ref ModelVertex vertex, Vector3 t, Vector3 b, Vector3 n)
        {
            vertex.Tangent = t;
            vertex.Binormal = b;
            vertex.Normal = n;
        }
    }
}
