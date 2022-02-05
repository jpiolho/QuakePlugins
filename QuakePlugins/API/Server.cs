﻿using QuakePlugins.Engine;
using QuakePlugins.Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    /// <apiglobal />
    public static class Server
    {
        /// <summary>
        /// Gets the current map name
        /// </summary>
        public static string Map
        {
            get { unsafe { return QEngine.StringGet(QEngine.GetGlobals()->mapname); } }
        }

        /// <summary>
        /// Gets a specific client by their index
        /// </summary>
        public static ServerClient GetClient(int index)
        {
            unsafe
            {
                var serverStatic = QEngine.ServerStatic;
                if (index < 0 || index > serverStatic->maxclients)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return new ServerClient((EngineClient*)(serverStatic->clients + (index * EngineClient.SizeOf)).ToPointer());
            }
        }

        /// <summary>
        /// Gets a list of all clients in the server. Active or not.
        /// </summary>
        public static ServerClient[] GetClients()
        {
            unsafe
            {
                var serverStatic = QEngine.ServerStatic;

                var array = new ServerClient[serverStatic->maxclients];

                IntPtr client = serverStatic->clients;
                for (int i = 0; i < array.Length; i++,client += EngineClient.SizeOf)
                    array[i] = new ServerClient((EngineClient*)client);

                return array;
            }
        }
    }
}
