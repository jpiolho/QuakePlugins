using QuakePlugins.Engine;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace QuakePlugins.API
{
    /// <summary>
    /// Provides access to the engine builtins, which are the engine functions that can be called from QuakeC.
    /// </summary>
    /// <apiglobal />
    public class Builtins
    {
        internal class RegisteredBuiltin
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public CustomBuiltinHandler Handler { get; set; }
        }

        public const int Unspecified = -1;

        public delegate void CustomBuiltinHandler();
        private List<RegisteredBuiltin> _customBuiltins = new List<RegisteredBuiltin>();

        internal IEnumerable<RegisteredBuiltin> CustomBuiltins => _customBuiltins;

        public void RegisterCustomBuiltin(string name, CustomBuiltinHandler function)
        {
            RegisterCustomBuiltin(Unspecified, name, function);
        }

        public void RegisterCustomBuiltin(int index, string name, CustomBuiltinHandler function)
        {
            _customBuiltins.Add(new RegisteredBuiltin()
            {
                Index = index,
                Name = name,
                Handler = function
            });
        }


        private static void CallBuiltIn(int id, params object[] parameters) => CallBuiltIn<object>(id, parameters);
        /// <summary>
        /// Super inefficient method to call builtins
        /// </summary>
        private static TReturnType CallBuiltIn<TReturnType>(int id, params object[] parameters)
        {
            unsafe
            {
                QEngine.QCRegistersBackup();

                Utils.SetQCGenericParameters(parameters);

                QEngine.BuiltinCall(id);

                object retValue = null;

                if (typeof(TReturnType) == typeof(float))
                    retValue = QEngine.QCGetFloatValue(QEngine.QCValueOffset.Return);
                else if (typeof(TReturnType) == typeof(bool))
                    retValue = QEngine.QCGetFloatValue(QEngine.QCValueOffset.Return) != 0;
                else if (typeof(TReturnType) == typeof(int))
                    retValue = QEngine.QCGetIntValue(QEngine.QCValueOffset.Return);
                else if (typeof(TReturnType) == typeof(QCFunction))
                    retValue = new QCFunction(QEngine.QCGetFunction(QEngine.QCGetIntValue(QEngine.QCValueOffset.Return)));
                else if (typeof(TReturnType) == typeof(bool))
                    retValue = QEngine.QCGetFloatValue(QEngine.QCValueOffset.Return) != 0;
                else if (typeof(TReturnType) == typeof(Vector3))
                    retValue = QEngine.QCGetVectorValue(QEngine.QCValueOffset.Return);
                else if (typeof(TReturnType) == typeof(string))
                    retValue = QEngine.QCGetStringValue(QEngine.QCValueOffset.Return);
                else if (typeof(TReturnType) == typeof(Edict))
                {
                    retValue = new Edict(QEngine.QCGetEdictValue(QEngine.QCValueOffset.Return));
                }

                QEngine.QCRegistersRestore();

                return (TReturnType)retValue;
            }
        }

        /// <summary>
        /// Calls 'makevectors' engine builtin #1.
        /// </summary>
        public static void Makevectors(Vector3 vector) => CallBuiltIn(1, vector);
        /// <summary>
        /// Calls 'setorigin' engine builtin #2
        /// </summary>
        public static void SetOrigin(Edict entity, Vector3 origin) => CallBuiltIn(2, entity, origin);
        /// <summary>
        /// Calls 'setmodel' engine builtin #3
        /// </summary>
        public static void SetModel(Edict entity, string model) => CallBuiltIn(3, entity, model);
        /// <summary>
        /// Calls 'setsize' engine builtin #4
        /// </summary>
        public static void SetSize(Edict entity, Vector3 mins, Vector3 maxs) => CallBuiltIn(4, entity, mins, maxs);
        /// <summary>
        /// Calls 'break' engine builtin #6
        /// </summary>
        public static void Break() => CallBuiltIn(6);
        /// <summary>
        /// Calls 'random' engine builtin #7
        /// </summary>
        public static float Random() => CallBuiltIn<float>(7);
        /// <summary>
        /// Calls 'sound' engine builtin #8
        /// </summary>
        public static void Sound(Edict entity, float channel, string snd, float volume, float attenuation) => CallBuiltIn(8, entity, channel, snd, volume, attenuation);
        /// <summary>
        /// Calls 'normalize' engine builtin #9
        /// </summary>
        public static Vector3 Normalize(Vector3 vector) => CallBuiltIn<Vector3>(9, vector);
        /// <summary>
        /// Calls 'error' engine builtin #10
        /// </summary>
        public static void Error(string text) => CallBuiltIn(10, text);
        /// <summary>
        /// Calls 'objerror' engine builtin #11
        /// </summary>
        public static void ObjError(string text) => CallBuiltIn(11, text);
        /// <summary>
        /// Calls 'vlen' engine builtin #12
        /// </summary>
        public static float VLen(Vector3 vector) => CallBuiltIn<float>(12, vector);
        /// <summary>
        /// Calls 'vectoyaw' engine builtin #13
        /// </summary>
        public static float VecToYaw(Vector3 vector) => CallBuiltIn<float>(13, vector);
        /// <summary>
        /// Calls 'spawn' engine builtin #14
        /// </summary>
        public static Edict Spawn() => CallBuiltIn<Edict>(14);
        /// <summary>
        /// Calls 'remove' engine builtin #15
        /// </summary>
        public static void Remove(Edict entity) => CallBuiltIn(15, entity);
        /// <summary>
        /// Calls 'traceline' engine builtin #16
        /// </summary>
        public static void TraceLine(Vector3 v1, Vector3 v2, float nomonsters, Edict forent) => CallBuiltIn(16, v1, v2, nomonsters, forent);
        /// <summary>
        /// Calls 'checkclient' engine builtin #17
        /// </summary>
        public static Edict CheckClient() => CallBuiltIn<Edict>(17);
        // TODO: Placeholder for find #18
        /// <summary>
        /// Calls 'precache_sound' engine builtin #19
        /// </summary>
        public static string PrecacheSound(string snd) => CallBuiltIn<string>(19, snd);
        /// <summary>
        /// Calls 'precache_model' engine builtin #20
        /// </summary>
        public static string PrecacheModel(string model) => CallBuiltIn<string>(20, model);
        /// <summary>
        /// Calls 'stuffcmd' engine builtin #21
        /// </summary>
        public static void Stuffcmd(Edict entity, string command) => CallBuiltIn(21, entity, command);
        /// <summary>
        /// Calls 'findradius' engine builtin #22
        /// </summary>
        public static Edict FindRadius(Vector3 origin, float radius) => CallBuiltIn<Edict>(22, origin, radius);
        /// <summary>
        /// Calls 'bprint' engine builtin #23
        /// </summary>
        public static void BPrint(string text) => CallBuiltIn(23, text);
        /// <summary>
        /// Calls 'sprint' engine builtin #24
        /// </summary>
        public static void SPrint(Edict edict, string text) => CallBuiltIn(24, edict, text);
        /// <summary>
        /// Calls 'dprint' engine builtin #25
        /// </summary>
        public static void DPrint(string text) => CallBuiltIn(25, text);
        /// <summary>
        /// Calls 'ftos' engine builtin #26.
        /// </summary>
        public static string Ftos(float f) => CallBuiltIn<string>(26, f);
        /// <summary>
        /// Calls 'vtos' engine builtin #27
        /// </summary>
        public static string Vtos(float f) => CallBuiltIn<string>(27, f);
        /// <summary>
        /// Calls 'coredump' engine builtin #28
        /// </summary>
        public static void Coredump() => CallBuiltIn(28);
        /// <summary>
        /// Calls 'traceon' engine builtin #29
        /// </summary>
        public static void TraceOn() => CallBuiltIn(29);
        /// <summary>
        /// Calls 'traceoff' engine builtin #30
        /// </summary>
        public static void TraceOff() => CallBuiltIn(30);
        /// <summary>
        /// Calls 'eprint' engine builtin #31
        /// </summary>
        public static void EPrint(Edict e) => CallBuiltIn(31, e);
        /// <summary>
        /// Calls 'walkmove' engine builtin #32
        /// </summary>
        public static float WalkMove(float yaw, float dist) => CallBuiltIn<float>(32, yaw, dist);
        /// <summary>
        /// Calls 'droptofloor' engine builtin #34
        /// </summary>
        public static float DropToFloor() => CallBuiltIn<float>(34);
        /// <summary>
        /// Calls 'lightstyle' engine builtin #35
        /// </summary>
        public static void Lightstyle(float style, string value) => CallBuiltIn(35, style, value);
        /// <summary>
        /// Calls 'rint' engine builtin #36
        /// </summary>
        public static float RInt(float v) => CallBuiltIn<float>(36, v);
        /// <summary>
        /// Calls 'floor' engine builtin #37
        /// </summary>
        public static float Floor(float v) => CallBuiltIn<float>(37, v);
        /// <summary>
        /// Calls 'ceil' engine builtin #38
        /// </summary>
        public static float Ceil(float v) => CallBuiltIn<float>(38, v);
        /// <summary>
        /// Calls 'checkbottom' engine builtin #40
        /// </summary>
        public static float CheckBottom(Edict e) => CallBuiltIn<float>(40, e);
        /// <summary>
        /// Calls 'pointcontents' engine builtin #41
        /// </summary>
        public static float PointContents(Vector3 v) => CallBuiltIn<float>(41, v);
        /// <summary>
        /// Calls 'fabs' engine builtin #43
        /// </summary>
        public static float FAbs(float f) => CallBuiltIn<float>(43, f);
        /// <summary>
        /// Calls 'aim' engine builtin #44
        /// </summary>
        public static Vector3 Aim(Edict e, float speed) => CallBuiltIn<Vector3>(44, e, speed);
        /// <summary>
        /// Calls 'cvar' engine builtin #45
        /// </summary>
        public static float Cvar(string s) => CallBuiltIn<float>(45, s);
        /// <summary>
        /// Calls 'localcmd' engine builtin #46
        /// </summary>
        public static void Localcmd(string command) => CallBuiltIn(46, command);
        /// <summary>
        /// Calls 'nextent' engine builtin #47
        /// </summary>
        public static Edict NextEnt(Edict e) => CallBuiltIn<Edict>(47, e);
        /// <summary>
        /// Calls 'particle' engine builtin #48
        /// </summary>
        public static void Particle(Vector3 origin, Vector3 direction, float color, float count) => CallBuiltIn(48, origin, direction, color, count);
        /// <summary>
        /// Calls 'changeyaw' engine builtin #49
        /// </summary>
        public static void ChangeYaw() => CallBuiltIn(49);
        /// <summary>
        /// Calls 'vectoangles' engine builtin #51
        /// </summary>
        public static Vector3 VecToAngles(Vector3 v) => CallBuiltIn<Vector3>(51, v);
        /// <summary>
        /// Calls 'WriteByte' engine builtin #52
        /// </summary>
        public static void WriteByte(float to, float f) => CallBuiltIn(52, to, f);
        /// <summary>
        /// Calls 'WriteChar' engine builtin #53
        /// </summary>
        public static void WriteChar(float to, float f) => CallBuiltIn(53, to, f);
        /// <summary>
        /// Calls 'WriteShort' engine builtin #54
        /// </summary>
        public static void WriteShort(float to, float f) => CallBuiltIn(54, to, f);
        /// <summary>
        /// Calls 'WriteLong' engine builtin #55
        /// </summary>
        public static void WriteLong(float to, float f) => CallBuiltIn(55, to, f);
        /// <summary>
        /// Calls 'WriteCoord' engine builtin #56
        /// </summary>
        public static void WriteCoord(float to, float f) => CallBuiltIn(56, to, f);
        /// <summary>
        /// Calls 'WriteAngle' engine builtin #57
        /// </summary>
        public static void WriteAngle(float to, float f) => CallBuiltIn(57, to, f);
        /// <summary>
        /// Calls 'WriteString' engine builtin #58
        /// </summary>
        public static void WriteString(float to, string f) => CallBuiltIn(58, to, f);
        /// <summary>
        /// Calls 'WriteEntity' engine builtin #59
        /// </summary>
        public static void WriteEntity(float to, Edict f) => CallBuiltIn(59, to, f);
        /// <summary>
        /// Calls 'movetogoal' engine builtin #67
        /// </summary>
        public static void MoveToGoal(float step) => CallBuiltIn(67, step);
        /// <summary>
        /// Calls 'precache_file' engine builtin #68
        /// </summary>
        public static string PrecacheFile(string file) => CallBuiltIn<string>(68, file);
        /// <summary>
        /// Calls 'makestatic' engine builtin #69
        /// </summary>
        public static void MakeStatic(Edict entity) => CallBuiltIn(69, entity);
        /// <summary>
        /// Calls 'changelevel' engine builtin #70
        /// </summary>
        public static void ChangeLevel(string level) => CallBuiltIn(70, level);
        /// <summary>
        /// Calls 'cvar_set' engine builtin #72 
        /// </summary>
        public static void CvarSet(string cvar, string value) => CallBuiltIn(72, cvar, value);
        /// <summary>
        /// Calls 'centerprint' engine builtin #73
        /// </summary>
        public static void CenterPrint(Edict target, string text) => CallBuiltIn(73, target, text);
        /// <summary>
        /// Calls 'ambientsound' engine builtin #74
        /// </summary>
        public static void AmbientSound(Vector3 pos, string sample, float volume, float attenuation) => CallBuiltIn(74, pos, sample, volume, attenuation);
        /// <summary>
        /// Calls 'precache_model2' engine builtin #75
        /// </summary>
        public static string PrecacheModel2(string model) => CallBuiltIn<string>(75, model);
        /// <summary>
        /// Calls 'precache_sound2' engine builtin #76
        /// </summary>
        public static string PrecacheSound2(string sound) => CallBuiltIn<string>(76, sound);
        /// <summary>
        /// Calls 'precache_file2' engine builtin #77
        /// </summary>
        public static string PrecacheFile2(string file) => CallBuiltIn<string>(77, file);
        /// <summary>
        /// Calls 'setspawnparms' engine builtin #78
        /// </summary>
        public static void SetSpawnParameters(Edict edict) => CallBuiltIn(78, edict);
        /// <summary>
        /// Calls 'finaleFinished' engine builtin #79
        /// </summary>
        public static void FinaleFinished(Edict killer, Edict kilee) => CallBuiltIn(79, killer, kilee);
        /// <summary>
        /// Calls 'localsound' engine builtin #80
        /// </summary>
        public static void LocalSound(Edict entity, string snd) => CallBuiltIn(80, entity, snd);
        /// <summary>
        /// Calls 'draw_point' engine builtin #81
        /// </summary>
        public static void DrawPoint(Vector3 point, float colormap, float lifetime, bool depthtest) => CallBuiltIn(81, point, colormap, lifetime, depthtest);
        /// <summary>
        /// Calls 'draw_line' engine builtin #82
        /// </summary>
        public static void DrawLine(Vector3 start, Vector3 end, float colormap, float lifetime, bool depthtest) => CallBuiltIn(82, start, end, colormap, lifetime, depthtest);
        /// <summary>
        /// Calls 'draw_arrow' engine builtin #83
        /// </summary>
        public static void DrawArrow(Vector3 start, Vector3 end, float colormap, float size, float lifetime, bool depthtest) => CallBuiltIn(83, start, end, colormap, size, lifetime, depthtest);
        /// <summary>
        /// Calls 'draw_ray' engine builtin #84
        /// </summary>
        public static void DrawRay(Vector3 start, Vector3 direction, float length, float colormap, float size, float lifetime, bool depthtest) => CallBuiltIn(84, start, direction, length, colormap, size, lifetime, depthtest);
        /// <summary>
        /// Calls 'draw_circle' engine builtin #85
        /// </summary>
        public static void DrawCircle(Vector3 origin, float radius, float colormap, float lifetime, bool depthtest) => CallBuiltIn(85, origin, radius, colormap, lifetime, depthtest);
        /// <summary>
        /// Calls 'draw_bounds' engine builtin #86
        /// </summary>
        public static void DrawBounds(Vector3 min, Vector3 max, float colormap, float lifetime, bool depthtest) => CallBuiltIn(86, min, max, colormap, lifetime, depthtest);
        /// <summary>
        /// Calls 'draw_worldtext' engine builtin #87
        /// </summary>
        public static void DrawWorldText(string text, Vector3 origin, float size, float lifetime, bool depthtest) => CallBuiltIn(87, text, origin, size, lifetime, depthtest);
        /// <summary>
        /// Calls 'draw_sphere' engine builtin #88
        /// </summary>
        public static void DrawSphere(Vector3 origin, float radius, float colormap, float lifetime, bool depthtest) => CallBuiltIn(88, origin, radius, colormap, lifetime, depthtest);
        /// <summary>
        /// Calls 'draw_cylinder' engine builtin #89
        /// </summary>
        public static void DrawCylinder(Vector3 origin, float halfHeight, float radius, float colormap, float lifetime, bool depthtest) => CallBuiltIn(89, origin, halfHeight, radius, colormap, lifetime, depthtest);
        /// <summary>
        /// Calls 'centerprint2' engine builtin #90
        /// </summary>
        public static void CenterPrint2(Edict client, string text) => CallBuiltIn(90, text);
        /// <summary>
        /// Calls 'bprint2' engine builtin #91
        /// </summary>
        public static void BPrint2(string text) => CallBuiltIn(91, text);
        /// <summary>
        /// Calls 'sprint2' engine builtin #92
        /// </summary>
        public static void SPrint2(Edict client, string text) => CallBuiltIn(92, client, text);
        /// <summary>
        /// Calls 'checkextension' engine builtin #99
        /// </summary>
        public static bool CheckExtension(string extensionName) => CallBuiltIn<bool>(99, extensionName);

        /// <summary>
        /// Calls an engine builtin by index that returns void.
        /// </summary>
        public static void ByIndexVoid(int index, params object[] arguments) => CallBuiltIn(index, arguments);
        /// <summary>
        /// Calls an engine builtin by index that returns a string.
        /// </summary>
        public static string ByIndexString(int index, params object[] arguments) => CallBuiltIn<string>(index, arguments);
        /// <summary>
        /// Calls an engine builtin by index that returns a float.
        /// </summary>
        public static float ByIndexFloat(int index, params object[] arguments) => CallBuiltIn<float>(index, arguments);
        /// <summary>
        /// Calls an engine builtin by index that returns a vector.
        /// </summary>
        public static Vector3 ByIndexVector(int index, params object[] arguments) => CallBuiltIn<Vector3>(index, arguments);
        /// <summary>
        /// Calls an engine builtin by index that returns an edict.
        /// </summary>
        public static Edict ByIndexEdict(int index, params object[] arguments) => CallBuiltIn<Edict>(index, arguments);
        /// <summary>
        /// Calls an engine builtin by index that returns an int.
        /// </summary>
        public static int ByIndexInt(int index, params object[] arguments) => CallBuiltIn<int>(index, arguments);
        /// <summary>
        /// Calls an engine builtin by index that returns a bool.
        /// </summary>
        public static bool ByIndexBool(int index, params object[] arguments) => CallBuiltIn<bool>(index, arguments);

    }
}
