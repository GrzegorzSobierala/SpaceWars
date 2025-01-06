using System;
using UnityEngine;

namespace Game.Utility
{
    [Serializable]
    public struct Vector3Double
    {
        public double x;
        public double y;
        public double z;

        public Vector3Double(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3Double(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public static Vector3Double operator *(Vector3Double v, double scalar)
        {
            return new Vector3Double(v.x * scalar, v.y * scalar, v.z * scalar);
        }

        public static Vector3Double operator +(Vector3Double a, Vector3Double b)
        {
            return new Vector3Double(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3Double operator -(Vector3Double a, Vector3Double b)
        {
            return new Vector3Double(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public double Magnitude => Math.Sqrt(x * x + y * y + z * z);

        public Vector3Double Normalized => this * (1.0 / Magnitude);

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)x, (float)y, (float)z);
        }
    }
}