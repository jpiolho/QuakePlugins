using QuakePlugins.API;
using QuakePlugins.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins
{
    internal class Utils
    {
        public static IntPtr MarshalStringToHGlobalUTF8(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var ptr = Marshal.AllocHGlobal(bytes.Length+1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            
            // Set zero terminator
            unsafe
            {
                *(byte*)(ptr.ToInt64() + bytes.Length) = 0;
            }

            return ptr;
        }


        public static void SetQCGenericParameters(params object[] parameters)
        {
            QEngine.QCSetArgumentCount(parameters.Length);

            var offset = QEngine.QCValueOffset.Parameter0;
            for (var i = 0; i < parameters.Length && i < 8; i++, offset = (QEngine.QCValueOffset)((int)offset + ((int)QEngine.QCValueOffset.Parameter1 - (int)QEngine.QCValueOffset.Parameter0)))
            {
                // Special case for null
                if (parameters[i] == null)
                {
                    QEngine.QCSetIntValue(offset, 0);
                    break;
                }

                switch (parameters[i])
                {
                    case float valueFloat: QEngine.QCSetFloatValue(offset, valueFloat); break;
                    case string valueString: QEngine.QCSetStringValue(offset, valueString); break;
                    case Vector3 valueVector: QEngine.QCSetVectorValue(offset, valueVector); break;
                    case Edict valueEdict: unsafe { QEngine.QCSetEdictValue(offset, valueEdict.EngineEdict); } break;
                    case QCFunction valueFunction: QEngine.QCSetIntValue(offset, valueFunction.Index); break;
                    case int valueInt: QEngine.QCSetIntValue(offset, valueInt); break;
                    case bool valueBool: QEngine.QCSetFloatValue(offset, valueBool ? 1 : 0); break;
                    default:
                        throw new Exception($"Unsupported parameter type: {parameters[i].GetType()}");
                }
            }
        }
    }
}
