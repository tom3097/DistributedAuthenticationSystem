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

        long Timestamp { get; set; }

        string Hash { get; set; }

        int SequenceNumber { get; set; }

        OperationType Type { get; set; }

        string Data { get; set; }

        #endregion
    }
}