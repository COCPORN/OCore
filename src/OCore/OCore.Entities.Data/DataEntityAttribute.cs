using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Entities.Data
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class DataEntityAttribute : Attribute
    {
        public string Name { get; private set; }

        public DataEntityAttribute(string entityName)
        {
            Name = entityName;
        }
    }
}
