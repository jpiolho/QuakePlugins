using QuakePlugins.Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Core
{
    internal static class VectorExtensions
    {
        public static EngineVector3 ToEngineVector(this Vector3 vector)
        {
            return new EngineVector3()
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z
            };
        }

        public static Vector3 ToVector3(this EngineVector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}
