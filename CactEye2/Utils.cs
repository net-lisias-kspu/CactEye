
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CactEye2.InitialSetup;


namespace CactEye2
{
    public static class Utils
    {
        // Equivilent methods for the System.Random to match the Unity.Random
        //
        // UnityEngine.Random is a single sequence of random numbers, and unless a seed is used, will always return
        // the same value.
        // This code takes advantage of that fact by using it when in DEBUG mode, and
        // then using the System.Random with a time-based seed when in release mode
        public static int Range(this System.Random rnd, int low, int high)
        {
#if DEBUG
            return UnityEngine.Random.Range(low, high);
#else
            return rnd.Next(low, high);
#endif
        }
        public static float Range(this System.Random rnd, float low, float high)
        {
#if DEBUG
            return UnityEngine.Random.Range(low, high);
#else
                return (float)(rnd.NextDouble() * (high - low) + low);
#endif
        }

        //
        // Set of methods to safeload data from ConfigNodes
        //
        public static string SafeLoad(this ConfigNode node, string value, string oldvalue)
        {
            if (!node.HasValue(value))
            {
                Log.Info("SafeLoad string, node missing value: " + value + ", oldvalue: " + oldvalue);
                return oldvalue;
            }
            return node.GetValue(value);
        }

        public static bool SafeLoad(this ConfigNode node, string value, bool oldvalue)
        {
            if (!node.HasValue(value))
            {
                Log.Info("SafeLoad bool, node missing value: " + value + ", oldvalue: " + oldvalue);
                return oldvalue;
            }

            try { return bool.Parse(node.GetValue(value)); }
            catch { return oldvalue; }
        }

        public static ushort SafeLoad(this ConfigNode node, string value, ushort oldvalue)
        {
            if (!node.HasValue(value))
            {
                Log.Info("SafeLoad ushort, node missing value: " + value + ", oldvalue: " + oldvalue);
                return oldvalue;
            }
            try { return ushort.Parse(node.GetValue(value)); }
            catch { return oldvalue; }
        }

        public static int SafeLoad(this ConfigNode node, string value, int oldvalue)
        {
            if (!node.HasValue(value))
            {
                Log.Info("SafeLoad int, node missing value: " + value + ", oldvalue: " + oldvalue);
                return oldvalue;
            }
            try { return int.Parse(node.GetValue(value)); }
            catch { return oldvalue; }
        }

        public static float SafeLoad(this ConfigNode node, string value, float oldvalue)
        {
            if (!node.HasValue(value))
            {
                Log.Info("SafeLoad float, node missing value: " + value + ", oldvalue: " + oldvalue);
                return oldvalue;
            }
            try { return float.Parse(node.GetValue(value)); }
            catch { return oldvalue; }
        }

        public static double SafeLoad(this ConfigNode node, string value, double oldvalue)
        {
            if (!node.HasValue(value))
            {
                Log.Info("SafeLoad double, node missing value: " + value + ", oldvalue: " + oldvalue);
                return oldvalue;
            }
            try { return Double.Parse(node.GetValue(value)); }
            catch { return oldvalue; }
        }

        public static Guid SafeLoad(this ConfigNode node, string value, Guid oldvalue)
        {
            if (!node.HasValue(value))
            {
                Log.Info("SafeLoad Guid, node missing value: " + value + ", oldvalue: " + oldvalue);
                return oldvalue;
            }
            try { return Guid.Parse(node.GetValue(value)); }
            catch { return oldvalue; }
        }
    }
}
