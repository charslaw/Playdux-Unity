namespace AReSSO.Test
{
    public class TestStateNode : StateNode
    {
        public int Prop { get; }

        public TestStateNode(int prop)
        {
            Prop = prop;
        }
        private TestStateNode(TestStateNode old, PropertyChange<int> newProp) : base(old)
        {
            Prop = newProp.Else(old.Prop);
        }
        
        public TestStateNode Copy(PropertyChange<int> prop = default) =>
            new TestStateNode(this, prop);
        
        protected bool Equals(TestStateNode other)
        {
            return Prop == other.Prop;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestStateNode) obj);
        }

        public override int GetHashCode()
        {
            return Prop;
        }
    }
}