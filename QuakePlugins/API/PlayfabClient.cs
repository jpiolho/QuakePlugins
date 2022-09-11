using QuakePlugins.Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    public class PlayfabClient
    {
        private unsafe EnginePlayfabClient* _client;

        internal unsafe PlayfabClient(IntPtr client) : this((EnginePlayfabClient*)client.ToPointer()) { }
        internal unsafe PlayfabClient(EnginePlayfabClient* client)
        {
            _client = client;
        }


        public string Name
        {
            get
            {
                unsafe
                {
                    return Marshal.PtrToStringUTF8(_client->name, (int)_client->nameLength);
                }
            }
        }

        public string NetworkId
        {
            get
            {
                unsafe
                {
                    return Marshal.PtrToStringUTF8(_client->networkId, (int)_client->networkIdLength);
                }
            }
        }

        public string UniqueId
        {
            get
            {
                unsafe
                {
                    return Marshal.PtrToStringUTF8(_client->uniqueId, 16);
                }
            }
        }
    }
}
