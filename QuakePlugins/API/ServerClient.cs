using QuakePlugins.Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    /// <apitype />
    public class ServerClient
    {
        private unsafe EngineClient* _client;

        internal unsafe ServerClient(EngineClient* client)
        {
            _client = client;
        }


        public int Color
        {
            get
            {
                unsafe
                {
                    return _client->color;
                }
            }
        }

        public Edict Edict
        {
            get
            {
                unsafe
                {
                    return new Edict(_client->edict);
                }
            }
        }

        public bool Active
        {
            get
            {
                unsafe { return _client->active; }
            }
        }

        public string Name
        {
            get
            {
                unsafe
                {
                    return Marshal.PtrToStringAnsi(new IntPtr(_client->name));
                }
            }
        }
    }
}
