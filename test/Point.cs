using System;

namespace AReSSO.Test
{
    public class Point : IEquatable<Point>
    {
        public float X { get; }
        public float Y { get; }

        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        private Point(Point old, PropertyChange<float> newX, PropertyChange<float> newY)
        {
            X = newX.Else(old.X);
            Y = newY.Else(old.Y);
        }

        public Point Copy(PropertyChange<float> x = default, PropertyChange<float> y = default) =>
            new Point(this, x, y);

        public bool Equals(Point other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Point) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }
    }
}