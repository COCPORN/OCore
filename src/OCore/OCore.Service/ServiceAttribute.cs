using System;

namespace OCore.Service
{
    /// <summary>
    /// The Service attribute, indicating that the interface is
    /// of type GrainWithIntegerKey and should be exposed as an 
    /// external service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceAttribute : Attribute
    {
        public string Name { get; private set; }

        public ServiceAttribute(string serviceName)
        {
            Name = serviceName;
        }

    }
}
