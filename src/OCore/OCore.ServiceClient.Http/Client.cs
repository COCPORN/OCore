using Microsoft.Extensions.Options;
using OCore.Service;
using OCore.ServiceClient.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using DynamicProxyImplementation;

namespace OCore.ServiceClient.Http
{
    public class Client
    {
        HttpClient httpClient;
        ClientOptions options;

        public Client(HttpClient httpClient)
        {
            this.httpClient = httpClient;            
        }

        public T GetService<T>()
        {
            DynamicProxyFactory<HttpDynamicProxy<T>> factory = new DynamicProxyFactory<HttpDynamicProxy<T>>(new DynamicInterfaceImplementor());
            return factory.CreateDynamicProxy<T>(this);
        }

        public async Task<T> Invoke<T>(Type interfaceType, string method, params object[] parameters)
        {
            var serviceAttribute = (ServiceAttribute)interfaceType
               .GetCustomAttributes(true)
               .Where(attr => attr.GetType() == typeof(ServiceAttribute))
               .SingleOrDefault();

            var requestUri = CreateUri(options, serviceAttribute, interfaceType, method);

            var requestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod("POST"),
                RequestUri = new Uri(requestUri),
                Content = new StringContent(JsonSerializer.Serialize(parameters))
            };

            requestMessage.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await httpClient.SendAsync(requestMessage);
            var responseStatusCode = response.StatusCode;
            var responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<T>(responseBody);
            return responseObject;
        }

        public async Task<(TReturn, System.Net.HttpStatusCode)> Invoke<TInterface, TReturn>(string method, params object[] parameters)
        {
            var serviceAttribute = (ServiceAttribute)typeof(TInterface)
                .GetCustomAttributes(true)
                .Where(attr => attr.GetType() == typeof(ServiceAttribute))
                .SingleOrDefault();

            var requestUri = CreateUri(options, serviceAttribute, typeof(TInterface), method);

            var requestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod("POST"),
                RequestUri = new Uri(requestUri),
                Content = new StringContent(JsonSerializer.Serialize(parameters))
            };

            requestMessage.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await httpClient.SendAsync(requestMessage);
            var responseStatusCode = response.StatusCode;
            var responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<TReturn>(responseBody);
            return (responseObject, responseStatusCode);
        }

        private string CreateUri(ClientOptions options, ServiceAttribute serviceAttribute, Type serviceType, string method)
        {
            var serviceName = serviceAttribute?.Name ?? serviceType.Name;

            if (options == null)
            {
                options = new ClientOptions
                {
                    BaseUrl = "http://localhost:9000",
                    Prefix = "service"
                };
            }

            return $"{options.BaseUrl}/{options.Prefix}/{serviceName}/{method}";
        }
    }
}
