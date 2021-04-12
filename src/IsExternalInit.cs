// This file exists because `IsExternalInit` is not defined in our target (.net standard 2.0)
// See <https://developercommunity.visualstudio.com/t/error-cs0518-predefined-type-systemruntimecompiler/1244809>

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    // ReSharper disable once UnusedType.Global
    public static class IsExternalInit { }
}