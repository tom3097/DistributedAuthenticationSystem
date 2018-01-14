using DistributedAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Requests
{
    public class FatSynchronizationReq
    {
        #region properties

        public string SenderId { get; set; }

        public Operation[] History { get; set; }

        public Dictionary<string, long> SynchroTimes { get; set; }

        #endregion
    }
}