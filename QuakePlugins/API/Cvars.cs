using QuakePlugins.Engine;
using System;

namespace QuakePlugins.API
{
    internal class Cvars
    {
        public static Cvar Register(string name, string defaultValue, string description = "", int flags=0,float min=0,float max=1)
        {
            var ptr = QEngine.CvarGet(name);
            if (ptr != IntPtr.Zero)
                return new Cvar(ptr);

            return new Cvar(QEngine.CvarRegister(name, description, defaultValue, flags, min, max, false, IntPtr.Zero));
        }

        public static Cvar Get(string name)
        {
            return new Cvar(QEngine.CvarGet(name));
        }

        public static float GetFloatValue(IntPtr cvarReference, int defaultValue = 0)
        {
            return QEngine.CvarGetFloatValue(cvarReference, defaultValue);
        }

        public static bool GetBoolValue(IntPtr cvarReference, bool defaultValue = false)
        {
            return GetFloatValue(cvarReference, 0) >= 1;
        }

        public static string GetStringValue(IntPtr cvarReference)
        {
            return QEngine.CvarGetStringValue(cvarReference);
        }

    }
}
