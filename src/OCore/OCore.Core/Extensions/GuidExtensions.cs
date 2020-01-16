using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OCore.Core.Extensions
{
    public static class GuidExtensions
    {
        public static Guid Combine(this Guid guid, Guid other)
        {
            var myBytes = guid.ToByteArray();
            var otherBytes = other.ToByteArray();
            var output = new byte[myBytes.Length];

            for (var i = 0; i < myBytes.Length; i++)
            {
                output[i] = (byte)(myBytes[i] ^ otherBytes[i]);
            }

            return new Guid(output);
        }

        public static Guid ToGuid(this string @string, string @namespace = "")
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(@namespace + @string));
                return new Guid(hash);
            }
        }
    }
}
