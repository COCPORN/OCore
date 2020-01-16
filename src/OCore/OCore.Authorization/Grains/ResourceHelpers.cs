using OCore.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Authorization.Grains
{
    public static class ResourceHelpers
    {
        public static bool IsSuperResourceOf(string super, string resource)
        {
            var superParts = super.Split('/');
            var resourceParts = resource.Split('/');

            if (superParts.Length > resourceParts.Length)
                return false;

            for (var i = 0; i < superParts.Length; i++)
            {
                if (superParts[i] != resourceParts[i])
                    return false;
            }
            return true;
        }

        public static List<string> GetAllResources(string resource)
        {
            var retList = new List<string>();
            do
            {
                retList.Add(resource);
            } while (resource.TryChomp(out resource));
            return retList;
        }

    }
}
