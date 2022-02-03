using QuakePlugins.Core;
using QuakePlugins.Engine;
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
        public enum Value
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

        public static void Call(string name,params object[] parameters)
        {
            var func = QEngine.QCGetFunctionByName(name);
            QEngine.QCCallFunction(func);
        }

        public static float GetFloat(Value location)
        {
            return QEngine.QCGetFloatValue((QEngine.QCValueOffset)location);
        }

        public static int GetInt(Value location)
        {
            return QEngine.QCGetIntValue((QEngine.QCValueOffset)location);
        }

        public static Vector3 GetVector(Value location)
        {
            return QEngine.QCGetVectorValue((QEngine.QCValueOffset)location);
        }

        public static string GetString(Value location)
        {
            return QEngine.QCGetStringValue((QEngine.QCValueOffset)location);
        }

        public static Edict GetEdict(Value location)
        {
            unsafe
            {
                int offset = GetInt(location);
                var ptr = QEngine.EdictGetByOffset(offset);
                return new Edict(ptr);
            }
        }

        public static void SetFloat(Value location,float value)
        {
            QEngine.QCSetFloatValue((QEngine.QCValueOffset)location, value);
        }

        public static void SetInt(Value location,int value)
        {
            QEngine.QCSetIntValue((QEngine.QCValueOffset)location, value);
        }

        public static void SetString(Value location, string value)
        {
            QEngine.QCSetStringValue((QEngine.QCValueOffset)location, value);
        }

        public static void SetVector(Value location, Vector3 value)
        {
            QEngine.QCSetVectorValue((QEngine.QCValueOffset)location, value);
        }

        public static void SetEdict(Value location, Edict value)
        {
            unsafe
            {
                SetInt(location, QEngine.EdictGetOffset(value.EngineEdict));
            }
        }


        public static Edict Self
        {
            get
            {
                unsafe { return new Edict(QEngine.EdictGetByOffset(QEngine.GetGlobals()->self)); }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->self = QEngine.EdictGetOffset(value.EngineEdict); }
            }
        }

        public static Edict World
        {
            get
            {
                unsafe { return new Edict(QEngine.EdictGetByOffset(QEngine.GetGlobals()->world)); }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->world = QEngine.EdictGetOffset(value.EngineEdict); }
            }
        }

        public static Edict Other
        {
            get
            {
                unsafe { return new Edict(QEngine.EdictGetByOffset(QEngine.GetGlobals()->other)); }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->other = QEngine.EdictGetOffset(value.EngineEdict); }
            }
        }

        public static Edict MsgEntity
        {
            get
            {
                unsafe { return new Edict(QEngine.EdictGetByOffset(QEngine.GetGlobals()->msgEntity)); }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->msgEntity = QEngine.EdictGetOffset(value.EngineEdict); }
            }
        }

        public static Vector3 V_Forward
        {
            get
            {
                unsafe { return QEngine.GetGlobals()->v_forward.ToVector3(); }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->v_forward = value.ToEngineVector(); }
            }
        }

        public static Vector3 V_Up
        {
            get
            {
                unsafe { return QEngine.GetGlobals()->v_up.ToVector3(); }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->v_up = value.ToEngineVector(); }
            }
        }

        public static Vector3 V_Right
        {
            get
            {
                unsafe { return QEngine.GetGlobals()->v_right.ToVector3(); }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->v_right = value.ToEngineVector(); }
            }
        }

        public static float KilledMonsters
        {
            get
            {
                unsafe { return QEngine.GetGlobals()->killedMonsters; }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->killedMonsters = value; }
            }
        }

        public static string MapName
        {
            get
            {
                unsafe { return QEngine.StringGet(QEngine.GetGlobals()->mapname); }
            }
        }

        public static float Time
        {
            get
            {
                unsafe { return QEngine.GetGlobals()->time; }
            }
        }

        public static Vector3 TraceEndPosition
        {
            get
            {
                unsafe { return QEngine.GetGlobals()->traceEndPos.ToVector3(); }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->traceEndPos = value.ToEngineVector(); }
            }
        }

        public static Edict TraceEnt
        {
            get
            {
                unsafe { return new Edict(QEngine.EdictGetByOffset(QEngine.GetGlobals()->traceEnt)); }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->traceEnt = QEngine.EdictGetOffset(value.EngineEdict); }
            }
        }
        public static float TraceFraction
        {
            get
            {
                unsafe { return QEngine.GetGlobals()->traceFraction; }
            }

            set
            {
                unsafe { QEngine.GetGlobals()->traceFraction = value; }
            }
        }

    }
}
