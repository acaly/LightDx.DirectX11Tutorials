using LightDx.InputAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial07
{
    struct ModelVertex
    {
        [Position]
        public Vector3 Position;
        [TexCoord]
        public Vector2 TexutreCoord;
        [Normal]
        public Vector3 Normal;
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

            return ret;
        }
    }
}
