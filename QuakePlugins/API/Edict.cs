﻿using QuakePlugins.Core;
using QuakePlugins.Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    public class Edict
    {
        private int _edictIndex;
        private unsafe EngineEdict* _edict;

        public int Index => _edictIndex;
        public unsafe EngineEdict* EngineEdict => _edict;

        internal unsafe Edict(int index, EngineEdict* pointer)
        {
            _edictIndex = index;
            _edict = pointer;
        }


        public string Classname
        {
            get
            {
                unsafe
                {
                    return QEngine.StringGet(_edict->vars.classname);
                }
            }

            set
            {
                unsafe
                {
                    _edict->vars.classname = QEngine.StringCreate(value);
                }
            }
        }




        public void SetField(string name, string value) => InternalSetField(name, QEngine.StringCreate(value));
        public void SetField(string name, int value) => InternalSetField(name, value);
        public void SetField(string name, Vector3 value) => InternalSetField(name, new EngineVector3() { X = value.X, Y = value.Y, Z = value.Z });
        public void SetField(string name, float value) => InternalSetField(name, value);
        public void SetField(string name, Edict value)
        {
            unsafe
            {
                InternalSetField(name, QEngine.EdictGetOffset(value.EngineEdict));
            }
        }

        public string GetFieldString(string name) => QEngine.StringGet((int)InternalGetField(name));
        public int GetFieldInt(string name) => (int)InternalGetField(name);
        public float GetFieldFloat(string name) => (float)InternalGetField(name);
        public Vector3 GetFieldVector(string name)
        {
            var vec = (EngineVector3)InternalGetField(name);
            return new Vector3(vec.X, vec.Y, vec.Z);
        }
        public Edict GetFieldEdict(string name)
        {
            unsafe {
                var ptr = QEngine.EdictGetByOffset(GetFieldInt(name));
                return new Edict(0, ptr);
            }
        }


        private void InternalSetField<TValue>(string name, params TValue[] values) where TValue : unmanaged
        {
            unsafe
            {
                var field = EngineEntityVars.GetFieldByName(name);

                if (field == null)
                    return;

                var fieldOffset = Marshal.OffsetOf<EngineEntityVars>(field.Name);
                var varOffset = Marshal.OffsetOf<EngineEdict>("vars");

                for (int i = 0; i < values.Length; i++)
                    *(TValue*)((long)EngineEdict + varOffset.ToInt64() + fieldOffset.ToInt64() + (i * sizeof(int))) = values[i];
            }
        }
        private object InternalGetField(string name)
        {
            unsafe
            {
                return EngineEntityVars.GetFieldByName(name)?.GetValue(_edict->vars) ?? null;
            }
        }
    }
}
