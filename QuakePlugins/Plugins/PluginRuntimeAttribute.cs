﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Plugins
{
    public class PluginRuntimeAttribute : Attribute
    {
        public string Id { get; set; }

        public PluginRuntimeAttribute()
        {

        }
    }
}
