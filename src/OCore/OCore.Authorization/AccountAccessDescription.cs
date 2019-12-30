using OCore.Authorization.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Authorization
{
    public class AccountAccessDescription
    {
        public string Resource { get; set; }
        public Permissions Permissions { get; set; }
    }
}
