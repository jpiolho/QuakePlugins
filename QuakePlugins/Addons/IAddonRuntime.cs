using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Addons
{
    internal interface IAddonRuntime
    {
        internal event EventHandler<Exception> UnhandledException;

        internal void Load();
    }
}
