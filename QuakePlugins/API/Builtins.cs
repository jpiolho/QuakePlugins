using QuakePlugins.Core;
using System;
using System.Numerics;

namespace QuakePlugins.API
{
    public static class Builtins
    {
        private static void CallBuiltIn(int id, params object[] parameters) => CallBuiltIn<object>(id, parameters);
        /// <summary>
        /// Super inefficient method to call builtins
        /// </summary>
        private static TReturnType CallBuiltIn<TReturnType>(int id, params object[] parameters)
        {
            QEngine.QCRegistersBackup();

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
                    case int valueInt: QEngine.QCSetIntValue(offset, valueInt); break;
                    case bool valueBool: QEngine.QCSetFloatValue(offset, valueBool ? 1 : 0); break;
                    default:
                        throw new Exception($"Unsupported parameter type: {parameters[i].GetType()}");
                }
            }

            QEngine.BuiltinCall(id);

            object retValue = null;
            if (typeof(TReturnType) == typeof(float))
                retValue = QEngine.QCGetFloatValue(QEngine.QCValueOffset.Return);
            else if (typeof(TReturnType) == typeof(bool))
                retValue = QEngine.QCGetFloatValue(QEngine.QCValueOffset.Return) != 0;
            else if (typeof(TReturnType) == typeof(int))
                retValue = QEngine.QCGetIntValue(QEngine.QCValueOffset.Return);
            else if (typeof(TReturnType) == typeof(bool))
                retValue = QEngine.QCGetFloatValue(QEngine.QCValueOffset.Return) != 0;
            else if (typeof(TReturnType) == typeof(Vector3))
                retValue = QEngine.QCGetVectorValue(QEngine.QCValueOffset.Return);
            else if (typeof(TReturnType) == typeof(string))
                retValue = QEngine.QCGetStringValue(QEngine.QCValueOffset.Return);
            else if (typeof(TReturnType) == typeof(Edict))
            {
                unsafe
                {
                    retValue = new Edict(0, QEngine.QCGetEdictValue(QEngine.QCValueOffset.Return));
                }
            }
            else
            {
                throw new Exception($"Unsupported return type: {typeof(TReturnType)}");
            }

            QEngine.QCRegistersRestore();

            return (TReturnType)retValue;
        }
        public static void Makevectors(Vector3 vector) => CallBuiltIn(1, vector);

        public static void SetOrigin(Edict entity, Vector3 origin) => CallBuiltIn(2, entity, origin);

        public static void SetModel(Edict entity, string model) => CallBuiltIn(3, entity, model);
        public static void SetSize(Edict entity, Vector3 mins, Vector3 maxs) => CallBuiltIn(4, entity, mins, maxs);
        public static void Break() => CallBuiltIn(6);
        public static float Random() => CallBuiltIn<float>(7);
        public static void Sound(Edict entity, float channel, string snd, float volume, float attenuation) => CallBuiltIn(8, entity, channel, snd, volume, attenuation);
        public static Vector3 Normalize(Vector3 vector) => CallBuiltIn<Vector3>(9, vector);
        public static void Error(string text) => CallBuiltIn(10, text);
        public static void ObjError(string text) => CallBuiltIn(11, text);
        public static float VLen(Vector3 vector) => CallBuiltIn<float>(12, vector);
        public static float VecToYaw(Vector3 vector) => CallBuiltIn<float>(13, vector);
        public static Edict Spawn() => CallBuiltIn<Edict>(14);
        public static void Remove(Edict entity) => CallBuiltIn(15);
        public static void TraceLine(Vector3 v1, Vector3 v2, float nomonsters, Edict forent) => CallBuiltIn(16, v1, v2, nomonsters, forent);
        public static Edict CheckClient() => CallBuiltIn<Edict>(17);
        public static string PrecacheSound(string snd) => CallBuiltIn<string>(19, snd);
        public static string PrecacheModel(string model) => CallBuiltIn<string>(20, model);
        public static void Stuffcmd(Edict entity, string command) => CallBuiltIn(21, entity, command);
        public static Edict FindRadius(Vector3 origin, float radius) => CallBuiltIn<Edict>(22, origin, radius);
        public static void BPrint(string text) => CallBuiltIn(23, text);
        public static void SPrint(Edict edict, string text) => CallBuiltIn(24, edict, text);
        public static void DPrint(string text) => CallBuiltIn(25, text);
        public static string Ftos(float f) => CallBuiltIn<string>(26, f);
        public static string Vtos(float f) => CallBuiltIn<string>(27, f);
        public static void Coredump() => CallBuiltIn(28);
        public static void TraceOn() => CallBuiltIn(29);
        public static void TraceOff() => CallBuiltIn(30);
        public static void EPrint(Edict e) => CallBuiltIn(31, e);
        public static float WalkMove(float yaw, float dist) => CallBuiltIn<float>(32, yaw, dist);
        public static float DropToFloor() => CallBuiltIn<float>(34);
        public static void Lightstyle(float style, string value) => CallBuiltIn(35);

        public static float CheckBottom(Edict e) => CallBuiltIn<float>(40, e);
        public static float PointContents(Vector3 v) => CallBuiltIn<float>(41, v);

        public static Vector3 Aim(Edict e, float speed) => CallBuiltIn<Vector3>(44, e, speed);
        public static float Cvar(string s) => CallBuiltIn<float>(45, s);
        public static void Localcmd(string command) => CallBuiltIn(46, command);
        public static Edict NextEnt(Edict e) => CallBuiltIn<Edict>(47, e);
        public static void Particle(Vector3 origin, Vector3 direction, float color, float count) => CallBuiltIn(48, origin, direction, color, count);

        public static void WriteByte(float to, float f) => CallBuiltIn(52, to, f);
        public static void WriteChar(float to, float f) => CallBuiltIn(53, to, f);
        public static void WriteShort(float to, float f) => CallBuiltIn(54, to, f);
        public static void WriteLong(float to, float f) => CallBuiltIn(55, to, f);
        public static void WriteCoord(float to, float f) => CallBuiltIn(56, to, f);
        public static void WriteAngle(float to, float f) => CallBuiltIn(57, to, f);
        public static void WriteString(float to, string f) => CallBuiltIn(58, to, f);
        public static void WriteEntity(float to, Edict f) => CallBuiltIn(59, to, f);

        public static void CvarSet(string cvar, string value) => CallBuiltIn(72, cvar, value);

        public static void LocalSound(Edict entity, string snd) => CallBuiltIn(80, entity, snd);
        public static void DrawPoint(Vector3 point, float colormap, float lifetime, bool depthtest) => CallBuiltIn(81, point, colormap, lifetime, depthtest);
        public static void DrawLine(Vector3 start, Vector3 end, float colormap, float lifetime, bool depthtest) => CallBuiltIn(82, start, end, colormap, lifetime, depthtest);
        public static void DrawArrow(Vector3 start, Vector3 end, float colormap, float size, float lifetime, bool depthtest) => CallBuiltIn(83, start, end, colormap, size, lifetime, depthtest);
        public static void DrawRay(Vector3 start, Vector3 direction, float length, float colormap, float size, float lifetime, bool depthtest) => CallBuiltIn(84, start, direction, length, colormap, size, lifetime, depthtest);
        public static void DrawCircle(Vector3 origin, float radius, float colormap, float lifetime, bool depthtest) => CallBuiltIn(85, origin, radius, colormap, lifetime, depthtest);
        public static void DrawBounds(Vector3 min, Vector3 max, float colormap, float lifetime, bool depthtest) => CallBuiltIn(86, min, max, colormap, lifetime, depthtest);
        public static void DrawWorldText(string text, Vector3 origin, float size, float lifetime, bool depthtest) => CallBuiltIn(87, text, origin, size, lifetime, depthtest);
        public static void DrawSphere(Vector3 origin, float radius, float colormap, float lifetime, bool depthtest) => CallBuiltIn(88, origin, radius, colormap, lifetime, depthtest);
        public static void DrawCylinder(Vector3 origin, float halfHeight, float radius, float colormap, float lifetime, bool depthtest) => CallBuiltIn(89, origin, halfHeight, radius, colormap, lifetime, depthtest);


        public static void ByIndexVoid(int index, params object[] arguments) => CallBuiltIn(index, arguments);
        public static string ByIndexString(int index, params object[] arguments) => CallBuiltIn<string>(index, arguments);
        public static float ByIndexFloat(int index, params object[] arguments) => CallBuiltIn<float>(index, arguments);
        public static Vector3 ByIndexVector(int index, params object[] arguments) => CallBuiltIn<Vector3>(index, arguments);
        public static Edict ByIndexEdict(int index, params object[] arguments) => CallBuiltIn<Edict>(index, arguments);
        public static int ByIndexInt(int index, params object[] arguments) => CallBuiltIn<int>(index, arguments);
        public static bool ByIndexBool(int index, params object[] arguments) => CallBuiltIn<bool>(index, arguments);
    }
}
