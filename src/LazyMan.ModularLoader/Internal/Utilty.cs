using System;
using System.Collections.Generic;
using System.Text;

namespace LazyMan.ModularLoader
{
    internal static class Utilty
    {
        public static bool TextEq(this string? str1, string? str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
