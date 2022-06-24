using OCore.Authorization.Abstractions;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Authorization
{

    [Serializable]
    [GenerateSerializer]
    public class AccountAccessDescription
    {
        public string Resource { get; set; }
        public Permissions Permissions { get; set; }
    }
}
