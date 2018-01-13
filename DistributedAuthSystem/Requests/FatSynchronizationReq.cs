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

        public int SenderId { get; set; }

        public Operation[] History { get; set; }

        public Dictionary<int, long> SynchroTimes { get; set; }

        #endregion
    }
}