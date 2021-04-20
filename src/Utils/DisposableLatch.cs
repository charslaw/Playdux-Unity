using System;

namespace Playdux.src.Utils
{
    /// A utility to make it easy to set a value for a block of code and ensure that it gets set back when the block is exited, even in the case of an exception.
    /// Intended for use with the using statement.
    /// <example>
    /// var myFlag = false;
    /// using (new DisposableLatch(() => myFlag = true;, () => myFlag = false;)) {
    ///     Console.WriteLine("myFlag is true!");
    ///     throw new Exception();
    /// }
    /// Console.WriteLine("myFlag is properly set back to false, even though an exception was thrown!");
    /// </example>
    internal readonly struct DisposableLatch : IDisposable
    {
        private readonly Action unsetter;

        internal DisposableLatch(Action setter, Action unsetter)
        {
            this.unsetter = unsetter;
            setter();
        }

        public void Dispose() => unsetter();
    }
}