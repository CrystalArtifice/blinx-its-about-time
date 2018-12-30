using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TimeControl
{
    public static class Serialiser
    {

        public static byte[] GetBytes(object o)
        {
            Type t = o.GetType();
            //Debug.Log("Type of object " + o.ToString() + ": " + t);
            if (t == typeof(byte))
            {
                return BitConverter.GetBytes((byte)o);
            }
            else if (t == typeof(short))
            {
                return BitConverter.GetBytes((short)o);
            }
            else if (t == typeof(ushort))
            {
                return BitConverter.GetBytes((ushort)o);
            }
            else if (t == typeof(int))
            {
                return BitConverter.GetBytes((int)o);
            }
            else if (t == typeof(uint))
            {
                return BitConverter.GetBytes((uint)o);
            }
            else if (t == typeof(long))
            {
                return BitConverter.GetBytes((long)o);
            }
            else if (t == typeof(ulong))
            {
                return BitConverter.GetBytes((ulong)o);
            }
            else if (t == typeof(float))
            {
                return BitConverter.GetBytes((float)o);
            }
            else if (t == typeof(double))
            {
                return BitConverter.GetBytes((double)o);
            }
            else if (t == typeof(decimal))
            {
                return GetBytes((decimal)o);
            }else if(t == typeof(Vector2))
            {
                return GetBytes((Vector2)o);
            }
            else if (t == typeof(Vector3))
            {
                return GetBytes((Vector3)o);
            }
            else if (t == typeof(Quaternion))
            {
                return GetBytes((Quaternion)o);
            }
            return null;
        }

        public static object Deserialise(byte[] bytes, int startIndex, Type t)
        {
            int size;
            return Deserialise(bytes, startIndex, t, out size);
        }

        public static object Deserialise(byte[] bytes, int startIndex, Type t, out int size)
        {
            if (t == typeof(byte))
            {
                size = sizeof(byte);
                return bytes[startIndex];
            }
            else if (t == typeof(short))
            {
                size = sizeof(short);
                return ToShort(bytes, startIndex);
            }
            else if (t == typeof(ushort))
            {
                size = sizeof(ushort);
                return ToUShort(bytes, startIndex);
            }
            else if (t == typeof(int))
            {
                size = sizeof(int);
                return ToInt(bytes, startIndex);
            }
            else if (t == typeof(uint))
            {
                size = sizeof(uint);
                return ToUInt(bytes, startIndex);
            }
            else if (t == typeof(long))
            {
                size = sizeof(long);
                return ToLong(bytes, startIndex);
            }
            else if (t == typeof(ulong))
            {
                size = sizeof(ulong);
                return ToULong(bytes, startIndex);
            }
            else if (t == typeof(float))
            {
                size = sizeof(float);
                return ToFloat(bytes, startIndex);
            }
            else if (t == typeof(double))
            {
                size = sizeof(double);
                return ToDouble(bytes, startIndex);
            }
            else if (t == typeof(decimal))
            {
                size = sizeof(decimal);
                return ToDecimal(bytes, startIndex);
            }else if(t == typeof(Vector2))
            {
                size = 2 * sizeof(float);
                return ToVector2(bytes, startIndex);
            }
            else if (t == typeof(Vector3))
            {
                size = 3 * sizeof(float);
                return ToVector3(bytes, startIndex);
            }
            else if (t == typeof(Quaternion))
            {
                size = 4 * sizeof(float);
                return ToQuaternion(bytes, startIndex);
            }
            size = 0;
            return null;
        }

        public static int ToInt(byte[] bytes, int startIndex)
        {
            return BitConverter.ToInt32(bytes, startIndex);
        }

        public static uint ToUInt(byte[] bytes, int startIndex)
        {
            return BitConverter.ToUInt32(bytes, startIndex);
        }
        public static short ToShort(byte[] bytes, int startIndex)
        {
            return BitConverter.ToInt16(bytes, startIndex);
        }
        public static ushort ToUShort(byte[] bytes, int startIndex)
        {
            return BitConverter.ToUInt16(bytes, startIndex);
        }
        public static long ToLong(byte[] bytes, int startIndex)
        {
            return BitConverter.ToInt64(bytes, startIndex);
        }
        public static ulong ToULong(byte[] bytes, int startIndex)
        {
            return BitConverter.ToUInt64(bytes, startIndex);
        }
        public static float ToFloat(byte[] bytes, int startIndex)
        {
            return BitConverter.ToSingle(bytes, startIndex);
        }
        public static double ToDouble(byte[] bytes, int startIndex)
        {
            return BitConverter.ToDouble(bytes, startIndex);
        }
        public static Vector2 ToVector2(byte[] bytes, int startIndex)
        {
            return new Vector2(
                    ToFloat(bytes, startIndex),
                    ToFloat(bytes, startIndex + 4)
                );
        }
        public static Vector3 ToVector3(byte[] bytes, int startIndex)
        {
            return new Vector3(
                    ToFloat(bytes, startIndex),
                    ToFloat(bytes, startIndex + 4),
                    ToFloat(bytes, startIndex + 8)
                );
        }
        public static Quaternion ToQuaternion(byte[] bytes, int startIndex)
        {
            return new Quaternion(
                    ToFloat(bytes, startIndex),
                    ToFloat(bytes, startIndex + 4),
                    ToFloat(bytes, startIndex + 8),
                    ToFloat(bytes, startIndex + 12)
                );
        }
        public static decimal ToDecimal(byte[] bytes, int startIndex)
        {
            int[] bits = new int[4];

            for (int ii = 0; ii < 4; ii++)
            {
                bits[ii + startIndex] = BitConverter.ToInt32(bytes, ii * 4);
            }

            return new decimal(bits);
        }
        

        public static byte[] GetBytes(byte b)
        {
            return new byte[] { b }; // that was easy
        }

        public static byte[] GetBytes(sbyte sb)
        {
            return BitConverter.GetBytes(sb);
        }

        public static byte[] GetBytes(short s)
        {
            return BitConverter.GetBytes(s);
        }

        public static byte[] GetBytes(ushort us)
        {
            return BitConverter.GetBytes(us);
        }

        public static byte[] GetBytes(int i)
        {
            return BitConverter.GetBytes(i);
        }

        public static byte[] GetBytes(uint ui)
        {
            return BitConverter.GetBytes(ui);
        }

        public static byte[] GetBytes(float f)
        {
            return BitConverter.GetBytes(f);
        }

        public static byte[] GetBytes(double d)
        {
            return BitConverter.GetBytes(d);
        }

        public static byte[] GetBytes(long l)
        {
            return BitConverter.GetBytes(l);
        }

        public static byte[] GetBytes(ulong ul)
        {
            return BitConverter.GetBytes(ul);
        }


        public static byte[] GetBytes(decimal dec)
        {
            int[] bits = decimal.GetBits(dec);
            byte[] bytes = new byte[16];
            byte[] temp;
            for (int ii = 0; ii < 4; ii++)
            {
                temp = BitConverter.GetBytes(bits[ii]);
                bytes[ii * 4] = temp[0];
                bytes[ii * 4 + 1] = temp[1];
                bytes[ii * 4 + 2] = temp[2];
                bytes[ii * 4 + 3] = temp[3];
            }
            return bytes;
        }

        public static byte[] GetBytes(Vector2 v)
        {
            byte[] bytes = new byte[8];
            byte[] temp;

            for (int ii = 0; ii < 2; ii++)
            {
                temp = System.BitConverter.GetBytes(v[ii]);
                bytes[ii * 4] = temp[0];
                bytes[ii * 4 + 1] = temp[1];
                bytes[ii * 4 + 2] = temp[2];
                bytes[ii * 4 + 3] = temp[3];
            }
            return bytes;
        }

        public static byte[] GetBytes(Vector3 v)
        {
            byte[] bytes = new byte[12];
            byte[] temp;

            for (int ii = 0; ii < 3; ii++)
            {
                temp = System.BitConverter.GetBytes(v[ii]);
                bytes[ii * 4] = temp[0];
                bytes[ii * 4 + 1] = temp[1];
                bytes[ii * 4 + 2] = temp[2];
                bytes[ii * 4 + 3] = temp[3];
            }
            return bytes;
        }

        public static byte[] GetBytes(Quaternion q)
        {
            byte[] bytes = new byte[16];
            byte[] temp;

            for (int ii = 0; ii < 4; ii++)
            {
                temp = System.BitConverter.GetBytes(q[ii]);
                bytes[ii * 4] = temp[0];
                bytes[ii * 4 + 1] = temp[1];
                bytes[ii * 4 + 2] = temp[2];
                bytes[ii * 4 + 3] = temp[3];
            }
            return bytes;
        }

        
    }
}