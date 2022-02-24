using QuakePlugins.API.LuaScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Core.Extensions
{
    internal static class HooksExtensions
    {
        public static bool ShouldStopProcessing(this Hooks.Handling handling)
        {
            return handling != Hooks.Handling.Continue;
        }
    }
}
