using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TreeStore.Common
{
    internal sealed class Guard
    {
        internal static Guard Against { get; } = new Guard();

        internal T Null<T>([NotNull] T? t, string paramName, [CallerMemberName] string callerMemberName = "") where T : class
        {
            if (t is null)
                throw new ArgumentNullException(paramName, $"In '{callerMemberName}'");

            return t;
        }

        internal string NullOrEmpty([NotNull] string? str, string paramName, [CallerMemberName] string callerMemberName = "")
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(paramName, $"In '{callerMemberName}'");

            return str;
        }
    }
}