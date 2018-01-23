using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace DistributedAuthSystem.States
{
    public class StreamData
    {
        #region properties

        public WebRequest Request { get; set; }

        public string Data { get; set; }

        #endregion
    }
}