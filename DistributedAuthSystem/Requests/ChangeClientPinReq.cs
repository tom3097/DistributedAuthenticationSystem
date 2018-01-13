using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Requests
{
    public class ChangeClientPinReq
    {
        public int CurrentPin { get; set; }

        public int NewPin { get; set; }
    }
}