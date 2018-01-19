using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Requests
{
    public class AuthPasswordReq
    {
        #region properties

        public string Pin { get; set; }

        public string OneTimePassword { get; set; }

        #endregion
    }
}