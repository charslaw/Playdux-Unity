namespace AReSSO.Test
{
    public class SimpleTestStateNode : StateNode
    {
        public int Prop { get; }

        public SimpleTestStateNode(int prop)
        {
            Prop = prop;
        }
        private SimpleTestStateNode(SimpleTestStateNode old, PropertyChange<int> newProp) : base(old)
        {
            Prop = newProp.Else(old.Prop);
        }
        
        public SimpleTestStateNode Copy(PropertyChange<int> prop = default) =>
            new SimpleTestStateNode(this, prop);
        
        protected bool Equals(SimpleTestStateNode other)
        {
            return Prop == other.Prop;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SimpleTestStateNode) obj);
        }

        public override int GetHashCode()
        {
            return Prop;
        }
    }
}