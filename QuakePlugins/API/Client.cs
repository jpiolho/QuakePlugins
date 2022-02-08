using QuakePlugins.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    internal class Client
    {
        /// <summary>
        /// Gets or sets the world entities data on the client side.
        /// Note that setting this value will probably leak some memory!
        /// </summary>
        public static string WorldEntitiesData
        {
            get
            {
                unsafe
                {
                    return Marshal.PtrToStringUTF8(new IntPtr(*(long*)(*(long*)QEngine.ClientWorldModel + 0x220)));
                }
            }

            set
            {
                unsafe
                {
                    var entitiesDataPointer = (*(long*)QEngine.ClientWorldModel + 0x220);

                    var ptr = Utils.MarshalStringToHGlobalUTF8(value);

                    *(char**)entitiesDataPointer = (char*)ptr.ToPointer();
                }
            }
        }
    }
}
