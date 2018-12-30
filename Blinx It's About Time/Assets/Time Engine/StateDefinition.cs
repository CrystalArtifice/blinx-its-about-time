using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;

namespace TimeControl
{
    public interface StateDefinition
    {
        Type Type { get; }

        byte[] GetDefaultState();
        byte[] GetState(object obj);
        void SetState(object obj, byte[] state);
    }


    /// <summary>
    /// Stores what properties of an object are recorded and set by the engine.
    /// </summary>
    /// <typeparam name="T">The type being defined.</typeparam>
    public struct StateDefinition<T> : StateDefinition
    {
        static readonly Type[] SUPPORTED_TYPES = new Type[] {
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Quaternion)
        };

        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }

        int serialisedArraySize;
        List<PropertyInfo> properties;

        public static StateDefinition CreateDefinition(params string[] recordProperties)
        {
            //Debug.Log("Creating definition for type " + typeof(T).ToString());
            StateDefinition<T> def = new StateDefinition<T>();

            PropertyInfo[] props = typeof(T).GetProperties();

            def.properties = new List<PropertyInfo>();
            def.serialisedArraySize = 0;
            foreach (PropertyInfo p in props)
            {
                if (Attribute.IsDefined(p, typeof(Recorded)) || recordProperties.Contains(p.Name))
                {
                    if (p.CanRead && p.CanWrite)
                    {
                        // check if type is supported
                        if (SUPPORTED_TYPES.Contains(p.PropertyType))
                        { 
                            def.properties.Add(p);
                            def.serialisedArraySize += RecordSize(p.PropertyType);
                        }
                    }
                }
            }
            return def;
        }



        /// <summary>
        /// Returns the number of bytes needed to record the given object.
        /// Throws an ArgumentException if an invalid type is given.
        /// </summary>
        /// <returns></returns>
        private static int RecordSize(Type t)
        {
            if (t == typeof(byte))
                return sizeof(byte);
            else if (t == typeof(short))
                return sizeof(short);
            else if (t == typeof(int))
                return sizeof(int);
            else if (t == typeof(uint))
                return sizeof(uint);
            else if (t == typeof(long))
                return sizeof(long);
            else if (t == typeof(ulong))
                return sizeof(ulong);
            else if (t == typeof(float))
                return sizeof(float);
            else if (t == typeof(double))
                return sizeof(double);
            else if (t == typeof(decimal))
                return sizeof(decimal);
            else if (t == typeof(Vector2))
                return 2 * sizeof(float);
            else if (t == typeof(Vector3))
                return 3 * sizeof(float);
            else if (t == typeof(Vector4))
                return 4 * sizeof(float);
            else if (t == typeof(Quaternion))
                return 4 * sizeof(float);
            
            throw new System.ArgumentException("Invalid type given: " + t);
        }

        /// <summary>
        /// Returns the default state of the type being defined, serialised into a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] GetDefaultState()
        {
            return GetState(default(T));
        }

        /// <summary>
        /// Returns the serialised state of the given object.
        /// </summary>
        /// <returns></returns>
        public byte[] GetState(object obj)
        {
            if (obj.GetType() != typeof(T))
            {
                throw new ArgumentException("Type of given object '" + obj.ToString() + "' is not compatible with definition type '" + typeof(T).ToString() + "'");
            }
            
            byte[] bytes = new byte[serialisedArraySize];
            object value;
            byte[] temp;
            int offset = 0;
            foreach (PropertyInfo p in properties)
            {
                value = p.GetValue(obj, null);
                temp = Serialiser.GetBytes(value);
                CopyBytes(temp, bytes, offset);
                offset += temp.Length;
            }
            return bytes;
        }

        /// <summary>
        /// Copies all bytes in the "from" array to the "to" array.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="idx"></param>
        private void CopyBytes(byte[] from, byte[] to, int idx)
        {
            for(int ii = 0; ii < from.Length; ii++)
            {
                to[ii + idx] = from[ii];
            }
        }

        /// <summary>
        /// Sets the state of a given object.
        /// </summary>
        /// <param name="state">The serialised state, from the GetState method.</param>
        /// <param name="obj">The object that will have its state set.</param>
        public void SetState(object obj, byte[] state)
        {
            if(obj.GetType() != typeof(T))
            {
                throw new ArgumentException("Type of given object '" + obj.ToString() + "' is not compatible with definition type '" + typeof(T).ToString() + "'");
            }
            if(state.Length != serialisedArraySize)
            {
                throw new ArgumentException("Given array has invalid length. Expected: " + serialisedArraySize + ", Received: " + state.Length);
            }

            // the index in the state buffer
            int idx = 0;
            int valSize;
            object deserredObject;
            foreach (PropertyInfo p in properties)
            {
                deserredObject = Serialiser.Deserialise(state, idx, p.PropertyType, out valSize);
                idx += valSize;
                p.SetValue(obj, deserredObject, null);
            }
        }

    }
}