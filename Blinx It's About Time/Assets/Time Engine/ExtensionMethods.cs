using System;

namespace TimeControl
{
    public enum ByteUnit
    {
        Byte, KB, MB, GB, TB, PB, EB, ZB, YB
    }
    public static class ExtensionMethods
    {

        public static string ToSize(this Int64 val, ByteUnit unit)
        {
            return (val / Math.Pow(1024, (Int64)unit)).ToString("0.00");
        }

        public static string ToSize(this Int32 val, ByteUnit unit)
        {
            return (val / Math.Pow(1024, (Int64)unit)).ToString("0.00");
        }

        public static string AutoToSize(this Int32 val)
        {
            ByteUnit unit = (ByteUnit)Math.Log(val, 1000);

            return val.ToSize(unit) + " " + unit;
        }
    }
}
