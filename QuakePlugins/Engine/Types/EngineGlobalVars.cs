using System.Runtime.InteropServices;

namespace QuakePlugins.Engine.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct EngineGlobalVars
    {
        public fixed int qcRegisters[28];        public int self;        public int other;        public int world;        public float time;        public float frametime;        public float forceRetouch;        public int mapname;        public float deathmatch;        public float coop;        public float teamplay;        public float serverflags;        public float totalSecrets;        public float totalMonsters;        public float killedMonsters;        public float parm1;        public float parm2;        public float parm3;        public float parm4;        public float parm5;        public float parm6;        public float parm7;        public float parm8;        public float parm9;        public float parm10;        public float parm11;        public float parm12;        public float parm13;        public float parm14;        public float parm15;        public float parm16;        public EngineVector3 v_forward;        public EngineVector3 v_up;        public EngineVector3 v_right;        public float traceAllSolid;        public float traceStartSolid;        public float traceFraction;        public EngineVector3 traceEndPos;        public EngineVector3 tracePlaneNormal;        public float tracePlaneDist;        public int traceEnt;        public float traceInOpen;        public float traceInWater;        public int msgEntity;        public int main;        public int startFrame;        public int playerPreThink;        public int playerPostThink;        public int clientKill;        public int clientConnect;        public int putClientInServer;        public int clientDisconnect;        public int setNewParms;        int setChangeParms;
    }
}
