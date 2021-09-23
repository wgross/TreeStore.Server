using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TreeStore.Common
{
    public static class Guard
    {
        public static void AgainstNull<T>([NotNull] T? t, string paramName, [CallerMemberName] string callerMemberName = "") where T : class
        {
            if (t is null)
                throw new ArgumentNullException(paramName, $"In method '{callerMemberName}'");
        }
    }
}