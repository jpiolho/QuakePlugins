using QuakePlugins.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    /// <apiglobal />
    public static class Game
    {
        /// <summary>
        /// Returns the current game. Eg: id1, mg1, rogue, fortress...
        /// </summary>
        public static string Mod => Path.GetFileName(QEngine.GameGetGameDir());
        /// <summary>
        /// Returns the full path to the mod folder.
        /// </summary>
        public static string ModFolder => QEngine.GameGetGameDir();
    }
}
