using QuakePlugins;
using QuakePlugins.Core;
using System;

namespace QuakePlugins.API.LuaScripting
{
    internal class Cvars
    {
        public static Cvar Register(string name, string defaultValue, string description = "")
        {
            return new Cvar(QEngine.CvarRegister(name, description, defaultValue, 0x0, 0f, 1f, false, System.IntPtr.Zero));
        }

        public static Cvar Get(string name)
        {
            return new Cvar(QEngine.CvarGet(name));
        }

        public static float GetFloatValue(IntPtr cvarReference,int defaultValue=0)
        {
            return QEngine.CvarGetFloatValue(cvarReference,defaultValue);
        }

        public static bool GetBoolValue(IntPtr cvarReference,bool defaultValue = false)
        {
            return GetFloatValue(cvarReference, 0) >= 1;
        }

        public static string GetStringValue(IntPtr cvarReference)
        {
            return QEngine.CvarGetStringValue(cvarReference);
        }

    }
}
