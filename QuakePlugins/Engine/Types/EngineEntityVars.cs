using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace QuakePlugins.Engine.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct EngineEntityVars
    {
        public float modelIndex;
        public EngineVector3 absmin;
        public EngineVector3 absmax;
        public float ltime;
        public float movetype;
        public float solid;
        public EngineVector3 origin;
        public EngineVector3 oldorigin;
        public EngineVector3 velocity;
        public EngineVector3 angles;
        public EngineVector3 avelocity;
        public EngineVector3 punchangle;
        public int classname;
        public int model;
        public float frame;
        public float skin;
        public float effects;
        public EngineVector3 mins;
        public EngineVector3 maxs;
        public EngineVector3 size;
        public int touch;
        public int use;
        public int think;
        public int blocked;
        public float nextthink;
        public int groundentity;
        public float health;
        public float frags;
        public float weapon;
        public int weaponmodel;
        public float weaponframe;
        public float currentammo;
        public float ammo_shells;
        public float ammo_nails;
        public float ammo_rockets;
        public float ammo_cells;
        public float items;
        public float takedamage;
        public int chain;
        public float deadflag;
        public EngineVector3 view_ofs;
        public float button0;
        public float button1;
        public float button2;
        public float impulse;
        public float fixangle;
        public EngineVector3 v_angle;
        public float idealpitch;
        public int netname;
        public int enemy;
        public float flags;
        public float colormap;
        public float team;
        public float max_health;
        public float teleport_time;
        public float armortype;
        public float armorvalue;
        public float waterlevel;
        public float watertype;
        public float ideal_yaw;
        public float yaw_speed;
        public int aiment;
        public int goalentity;
        public float spawnflags;
        public int target;
        public int targetname;
        public float dmg_take;
        public float dmg_save;
        public int dmg_inflictor;
        public int owner;
        public EngineVector3 movedir;
        public int message;
        public float sounds;
        public int noise;
        public int noise1;
        public int noise2;
        public int noise3;



        private static Dictionary<string, FieldInfo> _fieldNameDictionary;
        public static FieldInfo GetFieldByName(string name)
        {
            if (_fieldNameDictionary == null)
                _fieldNameDictionary = new Dictionary<string, FieldInfo>();

            if (_fieldNameDictionary.TryGetValue(name.ToUpperInvariant(), out var existingField))
                return existingField;

            var field = typeof(EngineEntityVars).GetFields(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (field != null)
                _fieldNameDictionary[name.ToUpperInvariant()] = field;

            return field;
        }
    }
}
