using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SuppressIndexingAttribute : Attribute
    {
    }
}
