﻿using QuakePlugins.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    /// <apiglobal />
    public static class Game
    {
        public static string Mod => QEngine.GameGetGameDir();
    }
}
