using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.ServiceClient.Http
{
    public class HttpDynamicProxy<T> : DynamicProxyImplementation.DynamicProxy
    {
        Client client;

        public HttpDynamicProxy(Client client)
        {
            this.client = client;
        }

        protected override bool TryGetMember(Type interfaceType, string name, out object result)
        {
            throw new NotImplementedException();
        }

        protected override bool TryInvokeMember(Type interfaceType, string name, object[] args, out object result)
        {
            Console.WriteLine($"TRYING TO INVOKE MEMBER {name} of {interfaceType.Name}");

            result = client.Invoke<T>(interfaceType, name, args);            
            return true;
        }

        protected override bool TrySetEvent(Type interfaceType, string name, object value)
        {
            throw new NotImplementedException();
        }

        protected override bool TrySetMember(Type interfaceType, string name, object value)
        {
            throw new NotImplementedException();
        }
    }
}
