#nullable enable
using System;
using System.Reflection;

namespace Playdux.src.Store
{
    public sealed class InitializeTypeMismatchException : Exception
    {
        public InitializeTypeMismatchException(MemberInfo givenType, MemberInfo expectedType)
            : base($"The given root state type ({givenType.Name}) does not match the expected root state type for this store ({expectedType.Name}).") { }
    }
}