using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Entities.Data.Extensions
{
    public static class EntityKeyExtensions
    {
        public static string GetEntityKey(this IGrain grain)
        {
            var primaryKeyString = grain.GetPrimaryKeyString();
            if (primaryKeyString.Contains(":"))
            {
                return grain.GetPrimaryKeyString().Split(':')[0];
            } else
            {
                return primaryKeyString;
            }
        }

        public static string GetEntityKeyExtension(this IGrain grain)
        {
            return grain.GetPrimaryKeyString().Split(':')[1];
        }

        public static (string, string) GetEntityCompoundKey(this IGrain grain)
        {
            var compound = grain.GetPrimaryKeyString().Split(':');
            return (compound[0], compound[1]);
        }
    }
}
