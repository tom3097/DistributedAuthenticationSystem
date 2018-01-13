using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Requests
{
    public class AuthorizaOperationReq
    {
        #region properties

        public int Pin { get; set; }

        public string OneTimePassword { get; set; }

        #endregion
    }
}