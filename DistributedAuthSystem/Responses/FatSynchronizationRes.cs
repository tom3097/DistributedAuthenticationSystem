using DistributedAuthSystem.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Responses
{
    public class FatSynchronizationRes
    {
        public string SenderId { get; set; }

        public FatSynchroResult Type { get; set; }

        public long SynchroTimestamp { get; set; }

        public Dictionary<string, long> SynchroTimes { get; set; }

        public long RequestTimestamp { get; set; }
    }
}