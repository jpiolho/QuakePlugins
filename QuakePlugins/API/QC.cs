using QuakePlugins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    public static class QC
    {
        public enum ValueLocation
        {
            Return = 1,
            Parameter0 = 4,
            Parameter1 = 7,
            Parameter2 = 10,
            Parameter3 = 13,
            Parameter4 = 16,
            Parameter5 = 19,
            Parameter6 = 22,
            Parameter7 = 25
        }

        public static float GetFloat(ValueLocation location)
        {
            return QEngine.QCGetFloatValue((QEngine.QCValueOffset)location);
        }

        public static int GetInt(ValueLocation location)
        {
            return QEngine.QCGetIntValue((QEngine.QCValueOffset)location);
        }

        public static Vector3 GetVector(ValueLocation location)
        {
            return QEngine.QCGetVectorValue((QEngine.QCValueOffset)location);
        }

        public static string GetString(ValueLocation location)
        {
            return QEngine.QCGetStringValue((QEngine.QCValueOffset)location);
        }

        public static Edict GetEdict(ValueLocation location)
        {
            unsafe
            {
                int offset = GetInt(location);
                var ptr = QEngine.EdictGetByOffset(offset);
                var num = QEngine.EdictGetIndex(offset);
                return new Edict(num, ptr);
            }
        }

        public static void SetFloat(ValueLocation location,float value)
        {
            QEngine.QCSetFloatValue((QEngine.QCValueOffset)location, value);
        }

        public static void SetInt(ValueLocation location,int value)
        {
            QEngine.QCSetIntValue((QEngine.QCValueOffset)location, value);
        }

        public static void SetString(ValueLocation location, string value)
        {
            QEngine.QCSetStringValue((QEngine.QCValueOffset)location, value);
        }

        public static void SetVector(ValueLocation location, Vector3 value)
        {
            QEngine.QCSetVectorValue((QEngine.QCValueOffset)location, value);
        }

        public static void SetEdict(ValueLocation location, Edict value)
        {
            unsafe
            {
                SetInt(location, QEngine.EdictGetOffset(value.EngineEdict));
            }
        }

    }
}
