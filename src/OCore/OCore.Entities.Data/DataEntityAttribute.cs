using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Entities.Data
{
    public enum KeyStrategy
    {
        /// <summary>
        /// The DataEntity will be universally available to anyone who knows the key
        /// </summary>
        Identity,

        /// <summary>
        /// The DataEntity will be universally available to all and have an implied ID of "Global".        
        /// </summary>
        Global,

        /// <summary>
        /// The DataEntity will be bound to the account Id (blah.com/data/userconfiguration)
        /// </summary>
        Account,

        /// <summary>
        /// The DataEntity Id will be prefixed with the account Id, and then accept an
        /// additional key
        /// </summary>
        AccountPrefix,

        /// <summary>
        /// The entity will be bound to the tenant ID, either from the API key or the account
        /// </summary>
        Tenant,

        /// <summary>
        /// The DataEntity Id will be prefixed with the tenant id, then accept an identity id
        /// </summary>
        TenantPrefix,

        /// <summary>
        /// The DataEntity will be a GUID combined from the account ID and a number of additional ids
        /// </summary>
        AccountCombined,

        /// <summary>
        /// The DataEntity will be a GUID combined from the account ID and a number of additional ids 
        /// and use that as a prefix
        /// </summary>
        AccountCombinedPrefix,

    }

    [Flags]
    public enum DataEntityMethods
    {
        None = 0,
        Create = 1,
        Read = 2,
        Update = 4,
        Delete = 8,
        All = Create | Read | Update | Delete
    }


    [AttributeUsage(AttributeTargets.Interface)]
    public class DataEntityAttribute : Attribute
    {
        public string Name { get; private set; }

        public KeyStrategy KeyStrategy { get; private set; }

        public DataEntityMethods DataEntityMethods { get; private set; }

        public DataEntityAttribute(string entityName,
            KeyStrategy keyStrategy = KeyStrategy.Identity,
            DataEntityMethods dataEntityMethods = DataEntityMethods.Read)
        {
            Name = entityName;
            KeyStrategy = keyStrategy;
            DataEntityMethods = dataEntityMethods;
        }
    }
}
