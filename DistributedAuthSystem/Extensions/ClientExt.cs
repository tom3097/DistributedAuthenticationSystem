using DistributedAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Extensions
{
    public static class ClientExt
    {
        public static void InitializePasswordLists(this Client client)
        {
            client.ActivatedList = new OneTimePasswordList();
            client.NonactivatedList = new OneTimePasswordList();
            client.ActivatedList.GeneratePasswords();
            client.NonactivatedList.GeneratePasswords();
        }
    }
}