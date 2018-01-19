using DistributedAuthSystem.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Responses
{
    public class ThinSynchronizationRes
    {
        public string SenderId { get; set; }

        public ThinSynchroResult Type { get; set; }

        public long SynchroTimestamp { get; set; }

        public Dictionary<string, long> SynchroTimes { get; set; }
    }
}