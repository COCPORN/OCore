using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Options
{
    public class DiagnosticsOptions
    {
        /// <summary>
        /// Whether or not to persist the contents of the CorrelationId recorder 
        /// </summary>
        public bool StoreCorrelationIdData { get; set; } = false;
    }
}
