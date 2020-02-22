using System;

namespace AReSSO
{
    /// <summary>
    /// Base class for all elements in the state tree.
    /// </summary>
    public abstract class StateNode
    {
        /// <summary>Unique, persistent ID for each state node. Shall not change when copying a state node.</summary>
        public Guid Id { get; }

        /// <summary>Default constructor. Generates a new ID for this node. Only for creating brand new nodes</summary>
        protected StateNode() => Id = Guid.NewGuid();

        /// <summary>
        /// Copy constructor. Must be called by inheritors in their copy constructors in order to preserve ID
        /// persistence through copy operations.
        /// </summary>
        protected StateNode(StateNode old) => Id = old.Id;

        /// <summary>Determines whether two state nodes are equal based on their Id</summary>
        public bool EqualsById(StateNode other) => Id.Equals(other.Id);
        
        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
    }
}