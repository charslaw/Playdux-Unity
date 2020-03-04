using System;
using System.Reflection;

namespace AReSSO.Store
{
    public class InitialStateTypeMismatchException : Exception
    {
        public InitialStateTypeMismatchException(MemberInfo givenType, MemberInfo expectedType)
            : base(
                $"The given root state type ({givenType.Name}) does not match" +
                $"the expected root state type for this store ({expectedType.Name})."
            )
        {
        }
    }
}