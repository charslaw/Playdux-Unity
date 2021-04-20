using System;

namespace Playdux.src.Store
{
    public enum SideEffectorType { Pre, Post }

    public class SideEffectorExecutionException : Exception
    {
        public SideEffectorType Type;

        internal SideEffectorExecutionException(SideEffectorType type, Exception inner)
            : base($"Side effector threw an exception during {type} effect", inner)
        {
            Type = type;
        }
    }
}