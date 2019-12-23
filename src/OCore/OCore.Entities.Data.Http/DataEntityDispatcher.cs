using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Entities.Data.Http
{
    public class GrainKey
    {
        public string Key { get; set; }

        public bool IsFanable { get; set; }
    }

    public abstract class DataEntityDispatcher
    {
        string prefix;
        string dataEntityName;
        KeyStrategy keyStrategy;

        public DataEntityDispatcher(string prefix, string dataEntityName, KeyStrategy keyStrategy)
        {
            this.prefix = prefix ?? string.Empty;
            if (this.prefix.EndsWith("/") == false)
            {
                this.prefix += "/";
            }
            this.dataEntityName = dataEntityName;
            this.keyStrategy = keyStrategy;
        }

        /// <summary>
        /// Get the RoutePattern for the current data entity based on key strategy
        /// </summary>
        /// <returns></returns>
        protected RoutePattern GetRoutePattern(string method = null)
        {
            string methodPostfix = method == null ? "" : $"/{method}";

            switch (keyStrategy)
            {
                case KeyStrategy.Identity:
                    return RoutePatternFactory.Parse($"{prefix}{dataEntityName}/{{identity}}{methodPostfix}");
            }
            return null;
        }

        /// <summary>
        /// Get the relevant key based on key strategy
        /// </summary>
        /// <returns></returns>
        protected GrainKey GetKey(HttpContext context)
        {
            switch (keyStrategy)
            {
                case KeyStrategy.Identity:
                    return new GrainKey
                    {
                        Key = context.Request.RouteValues["identity"].ToString(),
                        IsFanable = true
                    };
            }
            return null;
        }

    }
}
