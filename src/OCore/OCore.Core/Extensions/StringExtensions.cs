using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool TryChomp(this string str, out string result)
        {
            var index = str.LastIndexOf('/');
            if (index == -1)
            {
                result = str;
                return false;
            }
            result = str.Substring(0, index);
            return true;
        }
    }
}
