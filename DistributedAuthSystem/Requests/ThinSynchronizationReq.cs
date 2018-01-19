using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Requests
{
    public class ThinSynchronizationReq
    {
        public string SenderId { get; set; }

        public string LastHash { get; set; }

        public long SynchroTimestamp { get; set; }

        public Dictionary<string, long> SynchroTimes { get; set; }
    }
}