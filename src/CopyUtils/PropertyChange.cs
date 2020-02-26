namespace AReSSO.CopyUtils
{
    /// <summary>
    /// Represents a potentially changed property.
    /// </summary>
    public struct PropertyChange<T>
    {
        /// <summary>Whether or not the value this PropertyChange represents has actually been changed.</summary>
        public bool Changed { get; }
        /// <summary>The new value.</summary>
        private T Value { get; }

        private PropertyChange(bool changed, T value)
        {
            Changed = changed;
            Value = value;
        }

        /// <summary>
        /// If this represents a value that has changed, return the changed value, otherwise
        /// return the old value.
        /// </summary>
        public T Else(T old) => Changed ? Value : old;

        /// <summary>
        /// Implicitly wrap up a value in a PropertyChange. This makes it very fluent to specify a change.
        /// </summary>
        /// <remarks>
        /// This allows for usage like this:
        /// <code> myCopyableThing.Copy(prop: newVal); </code>
        /// Rather than this:
        /// <code> myCopyableThing.Copy(prop: new PropertyChange(newVal)); </code>
        /// </remarks>
        public static implicit operator PropertyChange<T>(T value) => new PropertyChange<T>(true, value);
    }
}