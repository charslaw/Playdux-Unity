using System;
using AReSSO.CopyUtils;

namespace AReSSO.Test
{
    internal class SimpleTestState : IEquatable<SimpleTestState>
    {
        public int N { get; }

        public SimpleTestState(int n)
        {
            N = n;
        }
        
        public SimpleTestState Copy(PropertyChange<int> n = default) => new SimpleTestState(n.Else(N));

        public bool Equals(SimpleTestState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return N == other.N;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SimpleTestState) obj);
        }

        public override int GetHashCode()
        {
            return N;
        }

        public override string ToString() => $"[ SimpleTestState {{N = {N}}} ]";
    }
    
    internal class Point : IEquatable<Point>
    {
        public float X { get; }
        public float Y { get; }

        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Point Copy(
            PropertyChange<float> x = default,
            PropertyChange<float> y = default) =>
            new Point(x.Else(X), y.Else(Y));

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
        
        public override string ToString() => $"[ Point {{X = {X}, Y = {Y}}} ]";
    }
}