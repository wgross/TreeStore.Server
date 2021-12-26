using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TreeStore.Common
{
    internal sealed class Guard
    {
        internal static Guard Against { get; } = new Guard();

#pragma warning disable CA1822 // Mark members as static

        internal T Null<T>([NotNull] T? t, string paramName, [CallerMemberName] string callerMemberName = "") where T : class
#pragma warning restore CA1822 // Mark members as static
        {
            if (t is null)
                throw new ArgumentNullException(paramName, $"In '{callerMemberName}'");

            return t;
        }

#pragma warning disable CA1822 // Mark members as static

        internal string NullOrEmpty([NotNull] string? str, string paramName, [CallerMemberName] string callerMemberName = "")
#pragma warning restore CA1822 // Mark members as static
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(paramName, $"In '{callerMemberName}'");

            return str;
        }
    }
}