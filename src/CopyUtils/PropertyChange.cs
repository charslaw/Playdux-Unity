namespace AReSSO.CopyUtils
{
    /// <summary>
    /// Represents the possibility of a changed property on a state object.
    /// </summary>
    public struct PropertyChange<T>
    {
        /// <summary>Whether or not the value this PropertyChange represents has actually been changed.</summary>
        private bool Changed { get; }
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
        ///
        /// This allows for usage like this:
        /// <code>
        ///     myCopyableThing.Copy(prop: newVal);
        /// </code>
        /// Rather than this:
        /// <code>
        ///     myCopyableThing.Copy(prop: new PropertyChange(newVal));
        /// </code>
        /// </summary>
        public static implicit operator PropertyChange<T>(T value) => new PropertyChange<T>(true, value);
    }
}