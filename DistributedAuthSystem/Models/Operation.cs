using DistributedAuthSystem.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Models
{
    public class Operation
    {
        #region fields

        public long Timestamp { get; set; }

        public string Hash { get; set; }

        public int SequenceNumber { get; set; }

        public OperationType Type { get; set; }

        public string Data { get; set; }

        #endregion
    }
}