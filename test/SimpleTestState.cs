namespace AReSSO.Test
{
    public class SimpleTestState
    {
        public int N { get; }

        public SimpleTestState(int n)
        {
            N = n;
        }
        private SimpleTestState(SimpleTestState old, PropertyChange<int> newN)
        {
            N = newN.Else(old.N);
        }
        
        public SimpleTestState Copy(PropertyChange<int> n = default) =>
            new SimpleTestState(this, n);
        
        protected bool Equals(SimpleTestState other)
        {
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
    }
}