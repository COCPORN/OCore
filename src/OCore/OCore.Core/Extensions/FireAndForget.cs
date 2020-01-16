using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Core.Extensions
{
    public static class ExtensionMethods
    {

        /// <summary>
        /// https://stackoverflow.com/questions/12803012/fire-and-forget-with-async-vs-old-async-delegate
        /// </summary>        
        public static async void FireAndForget(this Task task, ILogger logger = null)
        {
            try
            {                
                await task;
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogError(ex, "Fire and forget threw");
                }
            }
        }
    }
}
