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
        /// The DataEntity Id will be bound to the projected (tenant) account id
        /// </summary>
        ProjectedAccount,

        /// <summary>
        /// The DataEntity Id will be prefixed with the projected account id and accept an additional key
        /// </summary>
        ProjectedAccountPrefix,

        /// <summary>
        /// The DataEntity Id will be bound to the tenant of the projected account id
        /// </summary>
        ProjectedAccountTenant,

        /// <summary>
        /// The DataEntity Id will be prefixed with the tenant for the projected account id and accept an additional key
        /// </summary>
        ProjectedAccountTenantPrefix,

        /// <summary>
        /// The DataEntity id will be bound to the tenant from the api key
        /// </summary>
        ApiKeyTenant,

        /// <summary>
        /// The DataEntity will be prefixed with the tenant from the api key and then accept an additional key
        /// </summary>
        ApiKeyTenantPrefix,

        /// <summary>
        /// The DataEntity will be a GUID combined from the account ID and a number of additional ids
        /// </summary>
        AccountCombined,

        /// <summary>
        /// The DataEntity id will be a GUID combined from the projected account ID and a number of additional ids
        /// </summary>
        ProjectedAccountCombined,
    }


    [AttributeUsage(AttributeTargets.Interface)]
    public class DataEntityAttribute : Attribute
    {
        public string Name { get; private set; }

        public KeyStrategy KeyStrategy { get; private set; }

        public DataEntityAttribute(string entityName, KeyStrategy keyStrategy = KeyStrategy.Identity)
        {
            Name = entityName;
            KeyStrategy = keyStrategy;
        }
    }
}
